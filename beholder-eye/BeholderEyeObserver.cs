namespace beholder_eye
{
  using beholder_nest;
  using beholder_nest.Cache;
  using beholder_nest.Extensions;
  using beholder_nest.Mqtt;
  using Microsoft.Extensions.Logging;
  using MQTTnet;
  using System;
  using System.Collections.Generic;
  using System.Security.Cryptography;
  using System.Text.Json;
  using System.Threading;
  using System.Threading.Tasks;

  public class BeholderEyeObserver : IObserver<BeholderEyeEvent>
  {
    private readonly ILogger<BeholderEyeObserver> _logger;
    private readonly IBeholderMqttClient _beholderClient;
    private readonly ICacheClient _cacheClient;
    private readonly RedisCacheClient _redisCacheClient;
    private readonly HashAlgorithm _hashAlgorithm;

    private byte[] _lastPointerHash;

    public BeholderEyeObserver(ILogger<BeholderEyeObserver> logger, IBeholderMqttClient beholderClient, ICacheClient cacheClient, RedisCacheClient redisCacheClient, HashAlgorithm hashAlgorithm)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _beholderClient = beholderClient ?? throw new ArgumentNullException(nameof(beholderClient));
      _cacheClient = cacheClient ?? throw new ArgumentNullException(nameof(cacheClient));
      _redisCacheClient = redisCacheClient ?? throw new ArgumentNullException(nameof(redisCacheClient));
      _hashAlgorithm = hashAlgorithm ?? throw new ArgumentNullException(nameof(hashAlgorithm));
    }

    public void OnCompleted()
    {
      // Do Nothing
    }

    public void OnError(Exception error)
    {
      // Do Nothing
    }

    public void OnNext(BeholderEyeEvent eyeEvent)
    {
      switch (eyeEvent)
      {
        case AlignmentMapEvent alignmentMapEvent:
          HandleAlignmentMapGenerated(alignmentMapEvent.MatrixPixelLocations).Forget();
          break;
        case DesktopFrameEvent:
          // Do Nothing.
          break;
        case DesktopResizedEvent:
          // Do Nothing.
          break;
        case MatrixFrameEvent matrixFrameEvent:
          HandleMatrixFrameObserved(matrixFrameEvent.MatrixFrame).Forget();
          break;
        case RegionCaptureEvent regionCaptureEvent:
          HandleRegionCaptureEvent(regionCaptureEvent).Forget();
          break;
        case PointerImageEvent pointerImageEvent:
          HandlePointerImageObserved(pointerImageEvent.Image).Forget();
          break;
        case PointerPositionChangedEvent pointerPositionChangedEvent:
          HandlePointerPositionChanged(pointerPositionChangedEvent.PointerPosition).Forget();
          break;
        case ThumbnailImageEvent thumbnailImageEvent:
          HandleThumbnailImageObserved(thumbnailImageEvent.Key, thumbnailImageEvent.Image).Forget();
          break;
        default:
          _logger.LogWarning($"Unhandled or unknown BeholderEyeEvent: {eyeEvent}");
          break;
      }
    }

    public void Pulse()
    {
    }

    private async Task HandleMatrixFrameObserved(MatrixFrame matrixFrame)
    {
      try
      {
        await _beholderClient.MqttClient.PublishAsync(
            new MqttApplicationMessageBuilder()
                .WithTopic($"beholder/eye/{Environment.MachineName}/matrix_frame")
                .WithPayload(JsonSerializer.Serialize(matrixFrame))
                .Build(),
            CancellationToken.None
        );
      }
      catch (Exception)
      {
        // Do Nothing.
        // Serializing MatrixFrame.Data payloads of "Unknown" throw exceptions.
      }
    }

    private async Task HandleRegionCaptureEvent(RegionCaptureEvent captureEvent)
    {
      // Add the thumbnail image to redis
      var cacheKey = $"e/{Environment.MachineName}/region/{captureEvent.Name}";

      await _cacheClient.Base64ByteArraySet(cacheKey, captureEvent.Image);

      var captureInfo = new RegionCaptureInfo()
      {
        PrefrontalImageKey = cacheKey,
        X = captureEvent.RegionRectangle.X,
        Y = captureEvent.RegionRectangle.Y,
        Width = captureEvent.RegionRectangle.Width,
        Height = captureEvent.RegionRectangle.Height,
      };

      // Notify any subscribers that the region is available
      await _beholderClient
        .PublishEventAsync(
          $"beholder/eye/{{HOSTNAME}}/region/{captureEvent.Name}",
          captureInfo
        );
    }

    private async Task HandlePointerPositionChanged(PointerPosition pointerPosition)
    {
      await _beholderClient
        .PublishEventAsync(
          $"beholder/eye/{{HOSTNAME}}/pointer_position",
          pointerPosition
        );
    }

    private async Task HandlePointerImageObserved(byte[] pointerData)
    {
      var hash = _hashAlgorithm.ComputeHash(pointerData);
      if (_lastPointerHash == hash)
      {
        return;
      }

      _lastPointerHash = hash;

      var pointerImage = new PointerImage
      {
        Key = $"Eye_Pointer_{Convert.ToBase64String(hash)}.png",
        Image = $"data:image/png;base64,{Convert.ToBase64String(pointerData)}"
      };

      await _beholderClient
        .PublishEventAsync(
          $"beholder/eye/{{HOSTNAME}}/pointer_image",
          pointerImage
        );
    }

    private async Task HandleThumbnailImageObserved(string key, byte[] img)
    {
      // Add the thumbnail image to redis
      var cacheKey = $"e/{Environment.MachineName}/thumbnail";

      await _redisCacheClient.Base64ByteArraySet(cacheKey, img);
    }

    private async Task HandleAlignmentMapGenerated(IList<MatrixPixelLocation> map)
    {
      await _beholderClient.MqttClient.PublishAsync(
          new MqttApplicationMessageBuilder()
              .WithTopic($"beholder/eye/{Environment.MachineName}/alignment_map")
              .WithPayload(JsonSerializer.Serialize(map))
              .Build(),
          CancellationToken.None
      );
    }
  }
}