namespace beholder_eye
{
  using beholder_nest;
  using beholder_nest.Extensions;
  using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _cache;

    public BeholderEyeObserver(ILogger<BeholderEyeObserver> logger, IBeholderMqttClient beholderClient, IMemoryCache cache)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _beholderClient = beholderClient ?? throw new ArgumentNullException(nameof(beholderClient));
      _cache = cache ?? throw new ArgumentNullException(nameof(cache));
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

    private async Task HandlePointerPositionChanged(PointerPosition pointerPosition)
    {
      await _beholderClient.MqttClient.PublishAsync(
          new MqttApplicationMessageBuilder()
              .WithTopic($"beholder/eye/{Environment.MachineName}/pointer_position")
              .WithPayload(JsonSerializer.Serialize(pointerPosition))
              .Build(),
          CancellationToken.None
      );
    }

    private async Task HandlePointerImageObserved(string key, byte[] img)
    {
      var pointerImage = new PointerImage
      {
        Key = key,
        Image = $"data:image/png;base64,{Convert.ToBase64String(img)}"
      };

      await _beholderClient.MqttClient.PublishAsync(
          new MqttApplicationMessageBuilder()
              .WithTopic($"beholder/eye/{Environment.MachineName}/pointer_image")
              .WithPayload(JsonSerializer.Serialize(pointerImage))
              .Build(),
          CancellationToken.None
      );
    }

    private Task HandleThumbnailImageObserved(string key, byte[] img)
    {
      // Add the thumbnail image to the IMemoryCache
      var cacheKey = $"e/{Environment.MachineName}/thumbnail";
      _cache.Set(cacheKey, img);

      return Task.CompletedTask;

      // The following is commented out as we're now using the above to store the last thumbnail
      // image for the host. This is preferable as we're not causing the MQTT broker to have to
      // deal with thumbnail images, however, at the same time this requires a p2p connection
      // to the host -- it might be suitable to publish an event indicating a new thumbnail
      // image is available and what web host(s) it can be retrieved from.
      // Thusly, keeping this here for now.

      //And also publish on the message bus
      //var thumbnailImage = new ThumbnailImage
      //{
      //    Key = e.key,
      //    Image = $"data:image/png;base64,{Convert.ToBase64String(e.img)}"
      //};

      //await _mqttService.Publisher.PublishAsync(
      //    new MqttApplicationMessageBuilder()
      //        .WithTopic($"e/{Environment.MachineName}/thumbnail")
      //        .WithPayload(JsonSerializer.Serialize(thumbnailImage))
      //        .Build(),
      //    CancellationToken.None
      //);
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