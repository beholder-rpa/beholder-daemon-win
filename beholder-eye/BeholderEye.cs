namespace beholder_eye
{
  using beholder_nest.Extensions;
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
    private readonly ConcurrentDictionary<string, MatrixPixelLocation[]> _mapCache = new ConcurrentDictionary<string, MatrixPixelLocation[]>();
    private readonly ConcurrentDictionary<IObserver<BeholderEyeEvent>, BeholderEyeEventUnsubscriber> _observers = new ConcurrentDictionary<IObserver<BeholderEyeEvent>, BeholderEyeEventUnsubscriber>();
    private readonly object _alignLock = new object();

    private bool _streamDesktopThumbnails = true;
    private DesktopThumbnailStreamSettings _desktopThumbnailStreamSettings;

    private bool _watchPointerPosition = true;

    private bool _streamPointerImage = true;
    private PointerImageStreamSettings _pointerImageStreamSettings;

    private Dictionary<string, MatrixSettings> _matrixRegions;
    private Dictionary<string, ImageSettings> _focusRegions;

    public BeholderEye(ILogger<BeholderEye> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public AlignRequest AlignRequest
    {
      get;
      set;
    }

    /// <summary>
    /// Gets a value that indicates the status of the Beholder Eye
    /// </summary>
    public BeholderStatus Status
    {
      get;
      private set;
    }

    public IDisposable Subscribe(IObserver<BeholderEyeEvent> observer)
    {
      return _observers.GetOrAdd(observer, new BeholderEyeEventUnsubscriber(this, observer));
    }

    public Task ObserveWithUnwaveringSight(
        ObservationRequest spec,
        Func<string, IList<MatrixPixelLocation>> regionAlignmentMapCallback,
        CancellationToken token)
    {
      if (Status == BeholderStatus.Observing)
      {
        throw new InvalidOperationException("Beholder's Eye is already observing.");
      }

      // Validate the observation request and set mutatable instance settings.
      ValidateObservationRequest(spec);
      
      // Observe the screen on a seperate thread.
      return Task.Factory.StartNew(() =>
      {
        Status = BeholderStatus.Observing;
        var duplicatorInstance = new DesktopDuplicator(_logger, spec.AdapterIndex ?? 0, spec.DeviceIndex ?? 0);

        int? lastMatrixFrameIdSent = null;
        DateTime? lastDesktopThumbnailSent = null;
        DateTime? lastPointerImageSent = null;
        var lastRegionCaptureTimes = new ConcurrentDictionary<string, DateTime>();

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

          if (_streamDesktopThumbnails)
          {
            if (!lastDesktopThumbnailSent.HasValue || DateTime.Now.Subtract(lastDesktopThumbnailSent.Value) > TimeSpan.FromSeconds(_desktopThumbnailStreamSettings.MaxFps.Value))
            {
              var width = (int)Math.Ceiling(desktopFrame.DesktopWidth * _desktopThumbnailStreamSettings.ScaleFactor.Value);
              var height = (int)Math.Ceiling(desktopFrame.DesktopHeight * _desktopThumbnailStreamSettings.ScaleFactor.Value);

              var thumbnailImage = desktopFrame.GetThumbnailImage(width, height);

              var now = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
              var key = $"Eye_Thumb_{now}.png";

              OnBeholderEyeEvent(new ThumbnailImageEvent() { Key = key, Image = thumbnailImage });
            }
          }

          if (_watchPointerPosition)
          {
            var newPointerPosition = desktopFrame.PointerPosition with { };
            OnBeholderEyeEvent(new PointerPositionChangedEvent() { PointerPosition = newPointerPosition });
          }

          if (_streamPointerImage)
          {
            var pointerData = desktopFrame.GetPointerImage();
            if ((!lastPointerImageSent.HasValue
                      || DateTime.Now.Subtract(lastDesktopThumbnailSent.Value) > TimeSpan.FromSeconds(_pointerImageStreamSettings.MaxFps.Value))
                      && pointerData != null
                      && desktopFrame.PointerPosition.Visible == true)
            {
              OnBeholderEyeEvent(new PointerImageEvent() { Image = pointerData });
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

          foreach(var matrixRegion in _matrixRegions)
          {
            var matrixRegionName = matrixRegion.Key;
            var settings = matrixRegion.Value;

            if (settings.Map == null)
            {
              if (regionAlignmentMapCallback != null)
              {
                try
                {
                  settings.Map = regionAlignmentMapCallback.Invoke(matrixRegionName);
                }
                catch (Exception)
                {
                  _logger.LogError($"An exception occurred while calling the alignment map callback for region {matrixRegionName}");
                }
              }
            }

            // Optimize the map for data retrieval.
            if (!_mapCache.ContainsKey(matrixRegionName))
            {
              var map = settings.Map.ToArray();
              _mapCache.TryAdd(matrixRegionName, map);
            }

            var rawData = desktopFrame.DecodeMatrixFrameRaw(_mapCache[matrixRegionName]);
            var matrixFrame = MatrixFrame.CreateMatrixFrame(rawData, settings);

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
          }

          foreach (var focusRegion in _focusRegions)
          {
            var focusRegionName = focusRegion.Key;
            var settings = focusRegion.Value;

            if (lastRegionCaptureTimes.ContainsKey(focusRegionName) == false || DateTime.Now.Subtract(lastRegionCaptureTimes[focusRegionName]) > TimeSpan.FromSeconds(settings.MaxFps.Value))
            {
              var regionResult = desktopFrame.GetRegion(settings.X, settings.Y, settings.Width, settings.Height);
              if (regionResult.Item1 != null)
              {
                OnBeholderEyeEvent(new RegionCaptureEvent() { Name = focusRegionName, Image = regionResult.Item1, RegionRectangle = regionResult.Item2 });
                lastRegionCaptureTimes.AddOrUpdate(focusRegionName, DateTime.Now, (key, oldDate) => DateTime.Now);
              }
            }
          }
        };

        Status = BeholderStatus.NotObserving;
        _logger.LogInformation("The Beholder's eye has focused its attention elsewhere.");
      }, token, TaskCreationOptions.None, TaskScheduler.Default);
    }

    /// <summary>
    /// Provides a mechanism to add or update focus regions while the eye is currently observing.
    /// </summary>
    /// <param name="focusRegionName"></param>
    /// <param name="settings"></param>
    public void AddOrUpdateFocusRegion(string focusRegionName, ImageSettings settings)
    {
      if (Status == BeholderStatus.NotObserving)
      {
        throw new InvalidOperationException("The Beholder Eye is currently not observing. Set the desired focus regions of a new ObservationRequest and invoke ObserveWithUnwaveringSight with that request.");
      }

      ValidateAndUpdateFocusRegion(focusRegionName, settings);
    }

    /// <summary>
    /// Provids a mechanism to remove focus regions while the eye is currently observing.
    /// </summary>
    /// <param name="focusRegionName"></param>
    public void RemoveFocusRegion(string focusRegionName)
    {
      if (Status == BeholderStatus.NotObserving)
      {
        throw new InvalidOperationException("The Beholder Eye is currently not observing. Set the desired focus regions of a new ObservationRequest and invoke ObserveWithUnwaveringSight with that request.");
      }

      if (_focusRegions.ContainsKey(focusRegionName))
      {
        _focusRegions.Remove(focusRegionName);
      }
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

    /// <summary>
    /// Validates and ensures defaults are set on the observation specification
    /// </summary>
    /// <param name="spec"></param>
    private void ValidateObservationRequest(ObservationRequest spec)
    {
      if (spec == null)
      {
        throw new ArgumentNullException(nameof(spec));
      }

      // Check that focus regions are all good.
      _matrixRegions = new Dictionary<string, MatrixSettings>();
      _focusRegions = new Dictionary<string, ImageSettings>();

      if (spec.Regions == null)
      {
        spec.Regions = new List<ObservationRegion>();
      }

      foreach (var region in spec.Regions)
      {
        switch (region.Kind)
        {
          case ObservationRegionKind.MatrixFrame:
            if (region.MatrixSettings == null)
            {
              throw new ArgumentOutOfRangeException($"Region named {region.Name} was of kind MatrixFrame but did not specify matrix settings.");
            }
            if (region.MatrixSettings.DataFormat == null)
            {
              region.MatrixSettings.DataFormat = DataMatrixFormat.MatrixEvents;
            }

            _matrixRegions[region.Name] = region.MatrixSettings;
            break;
          case ObservationRegionKind.Image:
            ValidateAndUpdateFocusRegion(region.Name, region.BitmapSettings);
            break;
          default:
            throw new ArgumentOutOfRangeException($"Unknown or unsupported observation region kind: ${region.Kind}");
        }
      }

      _streamDesktopThumbnails = true;
      if (spec.StreamDesktopThumbnail.HasValue)
      {
        _streamDesktopThumbnails = spec.StreamDesktopThumbnail.Value;
      }

      _desktopThumbnailStreamSettings = new DesktopThumbnailStreamSettings();
      // Initialize desktop thumbnail stream settings from defaults or if supplied in request.
      if (_streamDesktopThumbnails)
      {
        if (spec.DesktopThumbnailStreamSettings != null)
        {
          if (spec.DesktopThumbnailStreamSettings.MaxFps.HasValue && spec.DesktopThumbnailStreamSettings.MaxFps.Value > 0)
          {
            _desktopThumbnailStreamSettings.MaxFps = spec.DesktopThumbnailStreamSettings.MaxFps.Value;
          }

          if (spec.DesktopThumbnailStreamSettings.ScaleFactor.HasValue && spec.DesktopThumbnailStreamSettings.ScaleFactor.Value > 0)
          {
            _desktopThumbnailStreamSettings.ScaleFactor = spec.DesktopThumbnailStreamSettings.ScaleFactor.Value;
          }
        }
      }

      _watchPointerPosition = true;
      if (spec.WatchPointerPosition.HasValue)
      {
        _watchPointerPosition = spec.WatchPointerPosition.Value;
      }

      _streamPointerImage = true;
      if (spec.StreamPointerImage.HasValue)
      {
        _streamPointerImage = spec.StreamPointerImage.Value;
      }

      _pointerImageStreamSettings = new PointerImageStreamSettings();

      // Initialize pointer image stream settings from defaults or if supplied in request.
      if (_streamPointerImage)
      {
        if (spec.PointerImageStreamSettings != null)
        {
          if (spec.PointerImageStreamSettings.MaxFps.HasValue && spec.PointerImageStreamSettings.MaxFps.Value > 0)
          {
            _pointerImageStreamSettings.MaxFps = spec.PointerImageStreamSettings.MaxFps.Value;
          }
        }
      }
    }

    private void ValidateAndUpdateFocusRegion(string focusRegionName, ImageSettings settings)
    {
      if (string.IsNullOrWhiteSpace(focusRegionName))
      {
        throw new ArgumentNullException(nameof(focusRegionName));
      }

      if (settings == null)
      {
        throw new ArgumentOutOfRangeException($"Region named {focusRegionName} was of kind Image but did not specify image settings.");
      }

      if (settings.MaxFps.HasValue == false)
      {
        settings.MaxFps = 0.25;
      }

      _focusRegions[focusRegionName] = settings;
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