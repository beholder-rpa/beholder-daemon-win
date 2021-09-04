namespace beholder_occipital.Controllers
{
  using beholder_nest.Attributes;
  using beholder_nest.Extensions;
  using beholder_occipital.Models;
  using Microsoft.Extensions.Logging;
  using MQTTnet;
  using System;
  using System.Text;
  using System.Text.Json;
  using System.Threading;
  using System.Threading.Tasks;

  [MqttController]
  public class BeholderOccipitalController
  {
    private readonly ILogger<BeholderOccipitalController> _logger;
    private readonly BeholderOccipital _occipitalLobe;

    private CancellationTokenSource _cts;
    private Task _lastTask;

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

      // If there's a previous request, cancel it.
      if (_cts != null)
        _cts.Cancel();

      // Create a CTS for this request.
      _cts = new CancellationTokenSource(750);

      // Observe the screen on a seperate thread.
      return _occipitalLobe.DetectObject(request);
    }
  }
}