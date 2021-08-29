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
  using System.Text.Json;
  using System.Threading;
  using System.Threading.Tasks;

  public class BeholderEyeObserver : IObserver<BeholderEyeEvent>
  {
    private readonly ILogger<BeholderEyeObserver> _logger;
    private readonly IBeholderMqttClient _beholderClient;
    private readonly ICacheClient _cacheClient;

    public BeholderEyeObserver(ILogger<BeholderEyeObserver> logger, IBeholderMqttClient beholderClient, ICacheClient cacheClient)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _beholderClient = beholderClient ?? throw new ArgumentNullException(nameof(beholderClient));
      _cacheClient = cacheClient ?? throw new ArgumentNullException(nameof(cacheClient));
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
          HandlePointerImageObserved(pointerImageEvent.Key, pointerImageEvent.Image).Forget();
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

      // Notify any subscribers that the region is available
      await _beholderClient
        .MqttClient
        .PublishEventAsync(
          BeholderConsts.PubSubName,
          $"beholder/eye/{Environment.MachineName}/region/{captureEvent.Name}",
          cacheKey
        );
    }

    private async Task HandlePointerPositionChanged(PointerPosition pointerPosition)
    {
      await _beholderClient
        .MqttClient
        .PublishEventAsync(
          BeholderConsts.PubSubName,
          $"beholder/eye/{Environment.MachineName}/pointer_position",
          pointerPosition
        );
    }

    private async Task HandlePointerImageObserved(string key, byte[] img)
    {
      var pointerImage = new PointerImage
      {
        Key = key,
        Image = $"data:image/png;base64,{Convert.ToBase64String(img)}"
      };

      await _beholderClient
        .MqttClient
        .PublishEventAsync(
          BeholderConsts.PubSubName,
          $"beholder/eye/{Environment.MachineName}/pointer_image",
          pointerImage
        );
    }

    private async Task HandleThumbnailImageObserved(string key, byte[] img)
    {
      // Add the thumbnail image to redis
      var cacheKey = $"e/{Environment.MachineName}/thumbnail";

      await _cacheClient.Base64ByteArraySet(cacheKey, img);
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