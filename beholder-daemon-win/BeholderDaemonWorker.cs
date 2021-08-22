namespace beholder_daemon_win
{
  using beholder_nest;
  using beholder_nest.Models;
  using beholder_nest.Mqtt;
  using Microsoft.Extensions.Hosting;
  using Microsoft.Extensions.Logging;
  using System;
  using System.Threading;
  using System.Threading.Tasks;

  public class BeholderDaemonWorker : BackgroundService
  {
    private readonly ILogger<BeholderDaemonWorker> _logger;
    private readonly IBeholderMqttClient _mqttClient;
    private readonly Lazy<BeholderServiceInfo> _serviceInfo = new Lazy<BeholderServiceInfo>(() =>
    {
      return new BeholderServiceInfo
      {
        ServiceName = "daemon",
        Version = "v1"
      };
    });

    public BeholderDaemonWorker(IBeholderMqttClient mqttClient, ILogger<BeholderDaemonWorker> logger)
    {
      _mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));
      _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      await _mqttClient.StartAsync();
      while (!stoppingToken.IsCancellationRequested)
      {
        await _mqttClient.MqttClient.PublishEventAsync(BeholderConsts.PubSubName, "beholder/ctaf", _serviceInfo.Value, cancellationToken: stoppingToken);
        _logger.LogInformation("Daemon Pulsed");
        await Task.Delay(5000, stoppingToken);
      }
    }
  }
}