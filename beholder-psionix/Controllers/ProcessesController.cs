namespace beholder_psionix.Controllers
{
  using beholder_nest.Attributes;
  using beholder_nest.Extensions;
  using beholder_nest.Mqtt;
  using Microsoft.Extensions.Logging;
  using MQTTnet;
  using System;
  using System.Linq;
  using System.Text.Json;
  using System.Threading;
  using System.Threading.Tasks;

  [MqttController]
  public class ProcessesController
  {
    private readonly ILogger<ProcessController> _logger;
    private readonly IMqttService _mqttService;
    private readonly BeholderPsionix _psionix;

    public ProcessesController(ILogger<ProcessController> logger, IMqttService mqttService, BeholderPsionix psionix)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _mqttService = mqttService ?? throw new ArgumentNullException(nameof(mqttService));
      _psionix = psionix ?? throw new ArgumentNullException(nameof(psionix));
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/get_processes")]
    public async Task GetProcesses(MqttApplicationMessage message)
    {
      var processes = _psionix
          .GetProcesses()
          .Take(25);

      await _mqttService.Publisher.PublishAsync(
          new MqttApplicationMessageBuilder()
              .WithTopic($"beholder/psionix/{Environment.MachineName}/process_list")
              .WithPayload(JsonSerializer.Serialize(processes))
              .Build(),
          CancellationToken.None
      );
    }
  }
}