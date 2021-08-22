namespace beholder_psionix
{
  using beholder_nest;
  using beholder_nest.Extensions;
  using Microsoft.Extensions.Logging;
  using MQTTnet;
  using System;
  using System.Text.Json;
  using System.Threading;
  using System.Threading.Tasks;

  public class BeholderPsionixObserver : IObserver<BeholderPsionixEvent>
  {
    private readonly ILogger<BeholderPsionixObserver> _logger;
    private readonly IBeholderMqttClient _beholderClient;

    public BeholderPsionixObserver(ILogger<BeholderPsionixObserver> logger, IBeholderMqttClient beholderClient)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _beholderClient = beholderClient ?? throw new ArgumentNullException(nameof(beholderClient));
    }

    public void OnCompleted()
    {
      // Do Nothing
    }

    public void OnError(Exception error)
    {
      // Do Nothing
    }

    public void OnNext(BeholderPsionixEvent psionixEvent)
    {
      switch (psionixEvent)
      {
        case ProcessChangedEvent processChangedEvent:
          HandleProcessChanged(processChangedEvent.ProcessInfo).Forget();
          break;
        default:
          _logger.LogWarning($"Unhandled or unknown BeholderPsionixEvent: {psionixEvent}");
          break;
      }
    }

    private async Task HandleProcessChanged(ProcessInfo processInfo)
    {
      await _beholderClient.MqttClient.PublishAsync(
          new MqttApplicationMessageBuilder()
              .WithTopic($"beholder/psionix/{Environment.MachineName}/process_changed")
              .WithPayload(JsonSerializer.Serialize(processInfo))
              .Build(),
          CancellationToken.None
          );
    }
  }
}