namespace beholder_eye
{
  using Microsoft.Extensions.Logging;
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Globalization;
  using System.Linq;
  using System.Security.Cryptography;
  using System.Threading;
  using System.Threading.Tasks;

  public class BeholderEye : IObservable<BeholderEyeEvent>
  {
    private readonly ILogger _logger;
    private readonly HashAlgorithm _hashAlgorithm;
    private readonly ConcurrentDictionary<string, MatrixPixelLocation[]> _mapCache = new ConcurrentDictionary<string, MatrixPixelLocation[]>();
    private readonly ConcurrentDictionary<IObserver<BeholderEyeEvent>, BeholderEyeEventUnsubscriber> _observers = new ConcurrentDictionary<IObserver<BeholderEyeEvent>, BeholderEyeEventUnsubscriber>();
    private object _alignLock = new object();

    public BeholderEye(ILogger<BeholderEye> logger, HashAlgorithm hashAlgorithm)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _hashAlgorithm = hashAlgorithm ?? throw new ArgumentNullException(nameof(hashAlgorithm));
    }

    public AlignRequest AlignRequest
    {
      get;
      set;
    }

    public IDisposable Subscribe(IObserver<BeholderEyeEvent> observer)
    {
      return _observers.GetOrAdd(observer, new BeholderEyeEventUnsubscriber(this, observer));
    }

    public Task ObserveWithUnwaveringSight(
        ObservationRequest observerInfo,
        Func<string, IList<MatrixPixelLocation>> regionAlignmentMapCallback,
        CancellationToken token)
    {
      if (observerInfo == null)
      {
        throw new ArgumentNullException(nameof(observerInfo));
      }

      // Check that matrix regions are all good.
      if (observerInfo.Regions != null)
      {
        foreach (var region in observerInfo.Regions)
        {
          if (region.Kind == ObservationRegionKind.MatrixFrame && region.MatrixSettings == null)
          {
            throw new ArgumentOutOfRangeException($"Region named {region.Name} was of kind MatrixFrame but did not specify matrix settings.");
          }

          if (region.MatrixSettings.DataFormat == null)
          {
            region.MatrixSettings.DataFormat = DataMatrixFormat.MatrixEvents;
          }
        }
      }

      var streamDesktopThumbnails = true;
      if (observerInfo.StreamDesktopThumbnail.HasValue)
      {
        streamDesktopThumbnails = observerInfo.StreamDesktopThumbnail.Value;
      }

      DesktopThumbnailStreamSettings desktopThumbnailStreamSettings = new DesktopThumbnailStreamSettings();
      // Initialize desktop thumbnail stream settings from defaults or if supplied in request.
      if (streamDesktopThumbnails)
      {
        if (observerInfo.DesktopThumbnailStreamSettings != null)
        {
          if (observerInfo.DesktopThumbnailStreamSettings.MaxFps.HasValue && observerInfo.DesktopThumbnailStreamSettings.MaxFps.Value > 0)
          {
            desktopThumbnailStreamSettings.MaxFps = observerInfo.DesktopThumbnailStreamSettings.MaxFps.Value;
          }

          if (observerInfo.DesktopThumbnailStreamSettings.ScaleFactor.HasValue && observerInfo.DesktopThumbnailStreamSettings.ScaleFactor.Value > 0)
          {
            desktopThumbnailStreamSettings.ScaleFactor = observerInfo.DesktopThumbnailStreamSettings.ScaleFactor.Value;
          }
        }
      }

      var watchPointerPosition = true;
      if (observerInfo.WatchPointerPosition.HasValue)
      {
        watchPointerPosition = observerInfo.WatchPointerPosition.Value;
      }

      var streamPointerImage = true;
      if (observerInfo.StreamPointerImage.HasValue)
      {
        streamPointerImage = observerInfo.StreamPointerImage.Value;
      }

      PointerImageStreamSettings pointerImageStreamSettings = new PointerImageStreamSettings();
      // Initialize pointer image stream settings from defaults or if supplied in request.
      if (streamPointerImage)
      {
        if (observerInfo.PointerImageStreamSettings != null)
        {
          if (observerInfo.PointerImageStreamSettings.MaxFps.HasValue && observerInfo.PointerImageStreamSettings.MaxFps.Value > 0)
          {
            pointerImageStreamSettings.MaxFps = observerInfo.PointerImageStreamSettings.MaxFps.Value;
          }
        }
      }

      // Observe the screen on a seperate thread.
      return Task.Factory.StartNew(() =>
      {
        var duplicatorInstance = new DesktopDuplicator(_logger, observerInfo.AdapterIndex ?? 0, observerInfo.DeviceIndex ?? 0);

        int? lastMatrixFrameIdSent = null;
        DateTime? lastDesktopThumbnailSent = null;
        PointerPosition lastPointerPosition = null;
        DateTime? lastPointerImageSent = null;

        int lastWidth = 0, lastHeight = 0;

        _logger.LogInformation("The cold stare of the Beholder's unwavering glare is now focused upon the screen...");

        foreach (var desktopFrame in duplicatorInstance.DuplicateDesktop(token))
        {
          if (token.IsCancellationRequested || desktopFrame == null || desktopFrame.DesktopWidth == 0 || desktopFrame.DesktopHeight == 0 || desktopFrame.IsDesktopImageBufferEmpty)
          {
            continue;
          }


          OnBeholderEyeEvent(new DesktopFrameEvent() { DesktopFrame = desktopFrame });

          if (lastWidth != desktopFrame.DesktopWidth || lastHeight != desktopFrame.DesktopHeight)
          {
            OnBeholderEyeEvent(new DesktopResizedEvent() { Width = desktopFrame.DesktopWidth, Height = desktopFrame.DesktopHeight });
            lastWidth = desktopFrame.DesktopWidth;
            lastHeight = desktopFrame.DesktopHeight;
          }

          if (streamDesktopThumbnails)
          {
            if (!lastDesktopThumbnailSent.HasValue || DateTime.Now.Subtract(lastDesktopThumbnailSent.Value) > TimeSpan.FromSeconds(desktopThumbnailStreamSettings.MaxFps.Value))
            {
              var width = (int)Math.Ceiling(desktopFrame.DesktopWidth * desktopThumbnailStreamSettings.ScaleFactor.Value);
              var height = (int)Math.Ceiling(desktopFrame.DesktopHeight * desktopThumbnailStreamSettings.ScaleFactor.Value);

              var thumbnailImage = desktopFrame.GetThumbnailImage(width, height);

              var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
              var key = $"Eye_Thumb_{now}.png";

              OnBeholderEyeEvent(new ThumbnailImageEvent() { Key = key, Image = thumbnailImage });
            }
          }

          if (watchPointerPosition)
          {
            var newPointerPosition = desktopFrame.PointerPosition;

            if (!lastPointerPosition.Equals(newPointerPosition))
            {
              OnBeholderEyeEvent(new PointerPositionChangedEvent() { PointerPosition = newPointerPosition });
              lastPointerPosition = newPointerPosition;
            }
          }

          if (streamPointerImage)
          {
            var pointerData = desktopFrame.GetPointerImage();
            if ((!lastPointerImageSent.HasValue
                      || DateTime.Now.Subtract(lastDesktopThumbnailSent.Value) > TimeSpan.FromSeconds(pointerImageStreamSettings.MaxFps.Value))
                      && pointerData != null
                      && desktopFrame.PointerPosition.Visible == true)
            {
              var hash = _hashAlgorithm.ComputeHash(pointerData);
              var key = $"Eye_Pointer_{Convert.ToBase64String(hash)}.png";

              OnBeholderEyeEvent(new PointerImageEvent() { Key = key, Image = pointerData });
            }
          }

          //// Double-check locking for a snapshot request.
          //if (SnapshotRequest != null)
          //{
          //    lock (_snapshotLock)
          //    {
          //        if (SnapshotRequest != null)
          //        {
          //            if (SnapshotRequest.ScaleFactor.HasValue == false)
          //            {
          //                SnapshotRequest.ScaleFactor = 1.0;
          //            }

          //            if (SnapshotRequest.Format.HasValue == false)
          //            {
          //                SnapshotRequest.Format = SnapshotFormat.Png;
          //            }

          //            var width = (int)Math.Ceiling(desktopFrame.DesktopWidth * SnapshotRequest.ScaleFactor.Value);
          //            var height = (int)Math.Ceiling(desktopFrame.DesktopHeight * SnapshotRequest.ScaleFactor.Value);

          //            var snapshot = desktopFrame.GetSnapshot(width, height, SnapshotRequest.Format.Value);

          //            var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
          //            var key = $"Eye_Snapshot_{now}";
          //            switch (SnapshotRequest.Format)
          //            {
          //                case SnapshotFormat.Jpeg:
          //                    key += ".jpg";
          //                    break;
          //                case SnapshotFormat.Png:
          //                default:
          //                    key += ".png";
          //                    break;
          //            }

          //            try
          //            {

          //                var db = Redis.GetDatabase();
          //                db.StringSet(key, snapshot, TimeSpan.FromHours(2));

          //                if (SnapshotRequest.Metadata != null)
          //                {
          //                    var metadataKey = $"Eye_Snapshot_Metadata_{now}";
          //                    db.StringSet(metadataKey, JsonSerializer.Serialize(SnapshotRequest.Metadata), TimeSpan.FromHours(2));
          //                }

          //                NexusConnection?.SendAsync("EyeReport", "Snapshot", new object[] { key, width, height });
          //            }
          //            catch (RedisException ex)
          //            {
          //                _logger.LogError(ex, $"Unable to store snapshot in redis. {ex.Message}");
          //            }

          //            // We've taken a snapshot, clear the request.
          //            SnapshotRequest = null;
          //        }
          //    }
          //}

          if (AlignRequest != null)
          {
            lock (_alignLock)
            {
              if (AlignRequest != null)
              {
                var pixelSize = 2;
                if (AlignRequest.PixelSize.HasValue && AlignRequest.PixelSize.Value > 0)
                {
                  pixelSize = AlignRequest.PixelSize.Value;
                }

                var map = desktopFrame.GenerateAlignmentMap(pixelSize);

                OnBeholderEyeEvent(new AlignmentMapEvent() { MatrixPixelLocations = map });

                AlignRequest = null;
              }
            }
          }

          if (observerInfo.Regions != null)
          {
            foreach (var region in observerInfo.Regions)
            {
              switch (region.Kind)
              {
                case ObservationRegionKind.MatrixFrame:
                  if (region.MatrixSettings.Map == null)
                  {
                    if (regionAlignmentMapCallback != null)
                    {
                      try
                      {
                        region.MatrixSettings.Map = regionAlignmentMapCallback.Invoke(region.Name);
                      }
                      catch (Exception)
                      {
                        _logger.LogError($"An exception occurred while calling the alignment map callback for region {region.Name}");
                      }
                    }
                  }

                  // Optimize the map for data retrieval.
                  if (!_mapCache.ContainsKey(region.Name))
                  {
                    var map = region.MatrixSettings.Map.ToArray();
                    _mapCache.TryAdd(region.Name, map);
                  }

                  var rawData = desktopFrame.DecodeMatrixFrameRaw(_mapCache[region.Name]);
                  var matrixFrame = MatrixFrame.CreateMatrixFrame(rawData, region.MatrixSettings);

                  // If we recieved a matrix frame, be sure that we haven't already seen it or that the frame counter has looped back around.
                  if (matrixFrame != null)
                  {
                    if (lastMatrixFrameIdSent.HasValue == false ||
                              matrixFrame.FrameId > lastMatrixFrameIdSent ||
                                  (lastMatrixFrameIdSent > matrixFrame.FrameId &&
                                   lastMatrixFrameIdSent - matrixFrame.FrameId > 1000)
                             )
                    {
                      OnBeholderEyeEvent(new MatrixFrameEvent() { MatrixFrame = matrixFrame });
                      lastMatrixFrameIdSent = matrixFrame.FrameId;
                    }
                  }
                  break;
                case ObservationRegionKind.Image:
                //TODO: Implement this.
                default:
                  break;
              }
            }
          }
        };

        _logger.LogInformation("The Beholder has focused its attention elsewhere.");
      }, token, TaskCreationOptions.None, TaskScheduler.Default);
    }

    /// <summary>
    /// Produces Beholder Eye Events
    /// </summary>
    /// <param name="beholderEyeEvent"></param>
    private void OnBeholderEyeEvent(BeholderEyeEvent beholderEyeEvent)
    {
      Parallel.ForEach(_observers.Keys, (observer) =>
      {
        try
        {
          observer.OnNext(beholderEyeEvent);
        }
        catch (Exception)
        {
          // Do Nothing.
        }
      });
    }

    #region Nested Classes
    private sealed class BeholderEyeEventUnsubscriber : IDisposable
    {
      private readonly BeholderEye _parent;
      private readonly IObserver<BeholderEyeEvent> _observer;

      public BeholderEyeEventUnsubscriber(BeholderEye parent, IObserver<BeholderEyeEvent> observer)
      {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _observer = observer ?? throw new ArgumentNullException(nameof(observer));
      }

      public void Dispose()
      {
        if (_observer != null && _parent._observers.ContainsKey(_observer))
        {
          _parent._observers.TryRemove(_observer, out _);
        }
      }
    }
    #endregion
  }
}