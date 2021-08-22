namespace beholder_psionix.Controllers
{
  using beholder_nest;
  using beholder_nest.Attributes;
  using beholder_nest.Mqtt;
  using Microsoft.Extensions.Logging;
  using MQTTnet;
  using System;
  using System.Text;
  using System.Text.Json;
  using System.Threading;
  using System.Threading.Tasks;

  [MqttController]
  public class ProcessController
  {
    private readonly ILogger<ProcessController> _logger;
    private readonly IMqttService _mqttService;
    private readonly BeholderPsionix _psionix;

    public ProcessController(ILogger<ProcessController> logger, IMqttService mqttService, BeholderPsionix psionix)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _mqttService = mqttService ?? throw new ArgumentNullException(nameof(mqttService));
      _psionix = psionix ?? throw new ArgumentNullException(nameof(psionix));
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/request_process")]
    public async Task RequestProcess(MqttApplicationMessage message)
    {
      var targetProcessName = Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
      var processInfo = _psionix.GetProcessInfo(targetProcessName);

      await _mqttService.Publisher.PublishAsync(
          new MqttApplicationMessageBuilder()
              .WithTopic($"beholder/psionix/{Environment.MachineName}/process")
              .WithPayload(JsonSerializer.Serialize(processInfo))
              .Build(),
          CancellationToken.None
          );
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/request_foreground_process")]
    public async Task RequestForegroundProcess(MqttApplicationMessage message)
    {
      var processInfo = _psionix.GetActiveProcess();

      await _mqttService.Publisher.PublishAsync(
          new MqttApplicationMessageBuilder()
              .WithTopic($"beholder/psionix/{Environment.MachineName}/foreground_process")
              .WithPayload(JsonSerializer.Serialize(processInfo))
              .Build(),
          CancellationToken.None
          );
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/request_observed_process")]
    public async Task RequestObservedProcesses(MqttApplicationMessage message)
    {
      var observedProceses = _psionix.GetObservedProcesses();

      await _mqttService.Publisher.PublishAsync(
          new MqttApplicationMessageBuilder()
              .WithTopic($"beholder/psionix/{Environment.MachineName}/observed_processes")
              .WithPayload(JsonSerializer.Serialize(observedProceses))
              .Build(),
          CancellationToken.None
      );
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/observe_process")]
    public async Task StartWatching(MqttApplicationMessage message)
    {
      var targetProcessName = Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
      _psionix.ObserveProcess(targetProcessName);

      await _mqttService.Publisher.PublishAsync(
          new MqttApplicationMessageBuilder()
              .WithTopic(BeholderConsts.BeholderNotificationBusTopic)
              .WithPayload($"Beholder Psionix started observing process {targetProcessName} on {Environment.MachineName}")
              .Build(),
          CancellationToken.None
      );
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/ignore_process")]
    public async Task StopWatching(MqttApplicationMessage message)
    {
      var targetProcessName = Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
      _psionix.IgnoreProcess(targetProcessName);

      await _mqttService.Publisher.PublishAsync(
          new MqttApplicationMessageBuilder()
              .WithTopic(BeholderConsts.BeholderNotificationBusTopic)
              .WithPayload($"Beholder Psionix stopped observing process {targetProcessName} on {Environment.MachineName}")
              .Build(),
          CancellationToken.None
      );
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/ensure_foreground_window")]
    public void EnsureForegroundWindow(MqttApplicationMessage message)
    {
      var targetProcessName = Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
      _psionix.EnsureForegroundWindow(targetProcessName);
    }
  }
}