namespace beholder_occipital.Controllers
{
  using beholder_nest.Attributes;
  using beholder_nest.Extensions;
  using beholder_occipital.Models;
  using Microsoft.Extensions.Logging;
  using MQTTnet;
  using System;
  using System.Collections.Concurrent;
  using System.Text;
  using System.Text.Json;
  using System.Threading;
  using System.Threading.Tasks;

  [MqttController]
  public class BeholderOccipitalController
  {
    private readonly ILogger<BeholderOccipitalController> _logger;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly BeholderOccipital _occipitalLobe;

    private readonly ConcurrentDictionary<string, Task> _throttles = new ConcurrentDictionary<string, Task>();

    public BeholderOccipitalController(ILogger<BeholderOccipitalController> logger, BeholderOccipital occipitalLobe, JsonSerializerOptions serializerOptions)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _occipitalLobe = occipitalLobe ?? throw new ArgumentNullException(nameof(occipitalLobe));
      _serializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));
    }

    [EventPattern("beholder/occipital/{HOSTNAME}/object_detection/detect")]
    public Task Detect(MqttApplicationMessage message)
    {
      //TODO: Change this to ICloudEvent<ObjectDetectionRequest>
      var requestJson = Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
      var doc = JsonDocument.Parse(requestJson);
      var rootElement = doc.RootElement.AddProperty("type", "SiftFlann");
      var request = JsonSerializer.Deserialize<ObjectDetectionRequest>(rootElement.ToString(), _serializerOptions);

      switch (request)
      {
        case SiftFlannObjectDetectionRequest siftFlann:
          PerformSiftFlannDetection(siftFlann);
          break;
        default:
          throw new InvalidOperationException($"Unknown or unsupported object detection type: {request}");
      }

      return Task.CompletedTask;
    }

    public void PerformSiftFlannDetection(SiftFlannObjectDetectionRequest request)
    {
      // Rate limit 
      if (_throttles.ContainsKey(request.QueryImagePrefrontalKey))
      {
        return;
      }

      // perform object detection
      var cts = new CancellationTokenSource(2000);
      var task = _occipitalLobe.DetectObject(request, cts.Token)
        .ContinueWith((t) =>
          _throttles.TryRemove(request.QueryImagePrefrontalKey, out Task _)
        );

      task.Forget();

      _throttles.TryAdd(request.QueryImagePrefrontalKey, task);
    }
  }
}