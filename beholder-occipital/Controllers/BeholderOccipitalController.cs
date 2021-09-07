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
    private readonly BeholderOccipital _occipitalLobe;

    private readonly ConcurrentDictionary<string, Task> _throttles = new ConcurrentDictionary<string, Task>();

    public BeholderOccipitalController(ILogger<BeholderOccipitalController> logger, BeholderOccipital occipitalLobe)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _occipitalLobe = occipitalLobe ?? throw new ArgumentNullException(nameof(occipitalLobe));
    }

    [EventPattern("beholder/occipital/{HOSTNAME}/object_detection/detect")]
    public Task Detect(MqttApplicationMessage message)
    {
      var requestJson = Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
      var request = JsonSerializer.Deserialize<ObjectDetectionRequest>(requestJson);

      // Rate limit 
      if (_throttles.ContainsKey(request.QueryImagePrefrontalKey))
      {
        return Task.CompletedTask;
      }

      // perform object detection
      var cts = new CancellationTokenSource(2000);
      var task = _occipitalLobe.DetectObject(request, cts.Token)
        .ContinueWith((t) =>
          _throttles.TryRemove(request.QueryImagePrefrontalKey, out Task _)
        );

      task.Forget();

      _throttles.TryAdd(request.QueryImagePrefrontalKey, task);
      return Task.CompletedTask;
    }
  }
}