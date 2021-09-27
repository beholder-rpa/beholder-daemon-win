namespace beholder_occipital
{
  using beholder_nest.Cache;
  using beholder_occipital.Models;
  using beholder_occipital.ObjectDetection;
  using Microsoft.Extensions.Logging;
  using OpenCvSharp;
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Point = OpenCvSharp.Point;

  public class BeholderOccipital : IObservable<BeholderOccipitalEvent>
  {
    private readonly IMatchMaskFactory _matchMaskFactory;
    private readonly IMatchProcessor _matchProcessor;

    private readonly ILogger<BeholderOccipital> _logger;
    private readonly ICacheClient _cacheClient;

    private readonly ConcurrentDictionary<IObserver<BeholderOccipitalEvent>, BeholderOccipitalEventUnsubscriber> _observers = new();

    public BeholderOccipital(IMatchMaskFactory matchMaskFactory, IMatchProcessor matchProcessor, ICacheClient cacheClient, ILogger<BeholderOccipital> logger)
    {
      _matchMaskFactory = matchMaskFactory ?? throw new ArgumentNullException(nameof(matchMaskFactory));
      _matchProcessor = matchProcessor ?? throw new ArgumentNullException(nameof(matchProcessor));

      _cacheClient = cacheClient ?? throw new ArgumentNullException(nameof(cacheClient));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IDisposable Subscribe(IObserver<BeholderOccipitalEvent> observer)
    {
      return _observers.GetOrAdd(observer, new BeholderOccipitalEventUnsubscriber(this, observer));
    }

    /// <summary>
    /// For the given Object Detection Request, determines if within the target image there exists one or more query images using feature detection.
    /// For each detected object, subscribed observers will be notified of the detected objects.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task DetectObject(SiftFlannObjectDetectionRequest request, CancellationToken cancellationToken = default)
    {
      if (string.IsNullOrWhiteSpace(request.QueryImagePrefrontalKey))
      {
        throw new InvalidOperationException("QueryImagePrefrontalKey must be specified");
      }

      if (string.IsNullOrWhiteSpace(request.TargetImagePrefrontalKey))
      {
        throw new InvalidOperationException("QueryImagePrefrontalKey must be specified");
      }

      _logger.LogTrace($"Performing Object Detection using {request.QueryImagePrefrontalKey}, {request.TargetImagePrefrontalKey}");

      return Task.Run(async () =>
      {
        var queryImageBytes = await _cacheClient.Base64ByteArrayGet(request.QueryImagePrefrontalKey);
        if (queryImageBytes == default)
        {
          _logger.LogError($"Query Image from Cache using key {request.QueryImagePrefrontalKey} was empty");
          return;
        }
        var targetImageBytes = await _cacheClient.Base64ByteArrayGet(request.TargetImagePrefrontalKey);
        if (targetImageBytes == default)
        {
          _logger.LogError($"Target Image from Cache using key {request.TargetImagePrefrontalKey} was empty");
          return;
        }

        var timing = new ObjectDetectionTiming();
        var startTime = Stopwatch.StartNew();

        var currentTime = Stopwatch.StartNew();
        // Decode the images
        var queryImage = Cv2.ImDecode(queryImageBytes, ImreadModes.Color);
        var trainImage = Cv2.ImDecode(targetImageBytes, ImreadModes.Color);

        if (queryImage.Empty())
        {
          throw new InvalidOperationException("The query image is empty.");
        }

        if (trainImage.Empty())
        {
          throw new InvalidOperationException("The train image is empty.");
        }
        currentTime.Stop();
        timing.DecodeTime = currentTime.ElapsedMilliseconds;

        currentTime.Restart();
        // If image pre-processors are specified, run them
        foreach (var imagePreProcessor in request.PreProcessors)
        {
          switch(imagePreProcessor)
          {
            case ScaleImageProcessor scaleImageProcessor:
              {
                var newQueryImageWidth = queryImage.Width * scaleImageProcessor.ScaleFactor.Value;
                var newQueryImageHeight = queryImage.Height * scaleImageProcessor.ScaleFactor.Value;

                queryImage = queryImage.Resize(new Size(newQueryImageWidth, newQueryImageHeight));

                var newTrainImageWidth = trainImage.Width * scaleImageProcessor.ScaleFactor.Value;
                var newTrainImageHeight = trainImage.Height * scaleImageProcessor.ScaleFactor.Value;

                trainImage = trainImage.Resize(new Size(newTrainImageWidth, newTrainImageHeight));
                break;
              }
          }
        }

        currentTime.Stop();
        timing.PreProcessingTime = currentTime.ElapsedMilliseconds;

        // Begin detection
        currentTime.Restart();
        var locationsResult = new List<IEnumerable<Point>>();

        DMatch[][] knn_matches = _matchProcessor.ProcessAndObtainMatches(queryImage, trainImage, out KeyPoint[] queryKeyPoints, out KeyPoint[] trainKeyPoints);

        if (request.MatchMaskSettings == null)
        {
          _matchMaskFactory.RatioThreshold = 0.76f;
          _matchMaskFactory.ScaleIncrement = 2.0f;
          _matchMaskFactory.RotationBins = 20;
        }
        else
        {
          _matchMaskFactory.RatioThreshold = request.MatchMaskSettings.RatioThreshold;
          _matchMaskFactory.ScaleIncrement = request.MatchMaskSettings.ScaleIncrement;
          _matchMaskFactory.RotationBins = request.MatchMaskSettings.RotationBins;
        }

        using var mask = _matchMaskFactory.CreateMatchMask(knn_matches, queryKeyPoints, trainKeyPoints, out var allGoodMatches);
        var goodMatches = new List<DMatch>(allGoodMatches);
        while (goodMatches.Count > 4 && cancellationToken.IsCancellationRequested == false)
        {
          // Use Homeography to obtain a perspective-corrected rectangle of the target in the query image.
          var sourcePoints = new Point2f[goodMatches.Count];
          var destinationPoints = new Point2f[goodMatches.Count];
          for (int i = 0; i < goodMatches.Count; i++)
          {
            DMatch match = goodMatches[i];
            sourcePoints[i] = queryKeyPoints[match.QueryIdx].Pt;
            destinationPoints[i] = trainKeyPoints[match.TrainIdx].Pt;
          }

          Point[] targetPoints = null;
          using var homography = Cv2.FindHomography(InputArray.Create(sourcePoints), InputArray.Create(destinationPoints), HomographyMethods.Ransac, 5.0);
          {
            if (homography.Rows > 0)
            {
              Point2f[] queryCorners = {
              new Point2f(0, 0),
              new Point2f(queryImage.Cols, 0),
              new Point2f(queryImage.Cols, queryImage.Rows),
              new Point2f(0, queryImage.Rows)
            };

              Point2f[] dest = Cv2.PerspectiveTransform(queryCorners, homography);
              targetPoints = new Point[dest.Length];
              for (int i = 0; i < dest.Length; i++)
              {
                targetPoints[i] = dest[i].ToPoint();
              }
            }
          }

          var matchesToRemove = new List<DMatch>();

          if (targetPoints != null)
          {
            locationsResult.Add(targetPoints);

            // Remove matches within bounding rectangle
            for (int i = 0; i < goodMatches.Count; i++)
            {
              DMatch match = goodMatches[i];
              var pt = trainKeyPoints[match.TrainIdx].Pt;
              var inPoly = Cv2.PointPolygonTest(targetPoints, pt, false);
              if (inPoly == 1)
              {
                matchesToRemove.Add(match);
              }
            }
          }

          // If we're no longer doing meaningful work, break out of the loop
          if (matchesToRemove.Count == 0)
          {
            break;
          }

          foreach (var match in matchesToRemove)
          {
            goodMatches.Remove(match);
          }
        }

        currentTime.Stop();
        timing.ObjectDetectionTime = currentTime.ElapsedMilliseconds;

        startTime.Stop();
        timing.OverallTime = startTime.ElapsedMilliseconds;

        await PostProcessResults(
          request,
          locationsResult,
          queryImage,
          trainImage,
          mask,
          queryKeyPoints,
          trainKeyPoints,
          allGoodMatches,
          timing,
          cancellationToken
        );

      }, cancellationToken);
    }

    /// <summary>
    /// Performs image post-processing
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    private Task PostProcessResults(
      SiftFlannObjectDetectionRequest request,
      IList<IEnumerable<Point>> locationsResult,
      Mat queryImage,
      Mat trainImage,
      Mat mask,
      KeyPoint[] queryKeyPoints,
      KeyPoint[] trainKeyPoints,
      IList<DMatch> allGoodMatches,
      ObjectDetectionTiming timing,
      CancellationToken cancellationToken = default)
    {
      // Perform any outputs
      if (locationsResult.Count > 0 && request.OutputSettings != null)
      {
        // Ensure the output folder exists
        Directory.CreateDirectory("./output/");
        var outputSettings = request.OutputSettings;

        // If DrawMatchesFilePrefix is specified, generate an output image and save it to the file system.
        if (!string.IsNullOrWhiteSpace(outputSettings.DrawMatchesFilePrefix) && !cancellationToken.IsCancellationRequested)
        {
          byte[] maskBytes = new byte[mask.Rows * mask.Cols];
          Cv2.Polylines(trainImage, locationsResult, true, new Scalar(255, 0, 0), 3, LineTypes.AntiAlias);
          using var outImg = new Mat();
          Cv2.DrawMatches(queryImage, queryKeyPoints, trainImage, trainKeyPoints, allGoodMatches, outImg, new Scalar(0, 255, 0), flags: DrawMatchesFlags.NotDrawSinglePoints);
          outImg.SaveImage($"./output/{outputSettings.DrawMatchesFilePrefix}");
        }
      }

      // Finally, create and notify subscribers of the result
      if (!cancellationToken.IsCancellationRequested)
      {
        var objectDetectionEvent = new ObjectDetectionEvent()
        {
          QueryImagePrefrontalKey = request.QueryImagePrefrontalKey,
          Timing = timing,
        };

        for (var i = 0; i < locationsResult.Count; i++)
        {
          var locationsResultPoly = locationsResult[i];

          // Filter out detections that have negative points
          if (locationsResultPoly.Any(poly => poly.X < 0 || poly.Y < 0))
          {
            continue;
          }

          var poly = new ObjectPoly();
          foreach (var locationResultPolyPoint in locationsResultPoly)
          {
            poly.Points.Add(new Models.Point()
            {
              X = locationResultPolyPoint.X,
              Y = locationResultPolyPoint.Y,
            });
          }

          objectDetectionEvent.Locations.Add(poly);
        }

        OnBeholderOccipitalEvent(objectDetectionEvent);
      }

      return Task.CompletedTask;
    }

    /// <summary>
    /// Produces Beholder Occipital Events
    /// </summary>
    /// <param name="beholderOccipitalEvent"></param>
    private void OnBeholderOccipitalEvent(BeholderOccipitalEvent beholderOccipitalEvent)
    {
      Parallel.ForEach(_observers.Keys, (observer) =>
      {
        try
        {
          observer.OnNext(beholderOccipitalEvent);
        }
        catch (Exception)
        {
          // Do Nothing.
        }
      });
    }

    #region Nested Classes
    private sealed class BeholderOccipitalEventUnsubscriber : IDisposable
    {
      private readonly BeholderOccipital _parent;
      private readonly IObserver<BeholderOccipitalEvent> _observer;

      public BeholderOccipitalEventUnsubscriber(BeholderOccipital parent, IObserver<BeholderOccipitalEvent> observer)
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
