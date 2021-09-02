namespace beholder_daemon_win
{
  using beholder_eye;
  using beholder_nest;
  using beholder_nest.Models;
  using beholder_nest.Mqtt;
  using beholder_occipital;
  using beholder_psionix;
  using Microsoft.Extensions.Hosting;
  using Microsoft.Extensions.Logging;
  using System;
  using System.Threading;
  using System.Threading.Tasks;

  public class BeholderDaemonWorker : BackgroundService
  {
    private readonly ILogger<BeholderDaemonWorker> _logger;
    private readonly IBeholderMqttClient _mqttClient;

    private readonly BeholderOccipital _occipital;
    private readonly IObserver<BeholderOccipitalEvent> _occipitalObserver;

    private readonly BeholderPsionix _psionix;
    private readonly IObserver<BeholderPsionixEvent> _psionixObserver;

    private readonly BeholderEye _eye;
    private readonly BeholderEyeObserver _eyeObserver;
    private readonly Lazy<BeholderServiceInfo> _serviceInfo = new Lazy<BeholderServiceInfo>(() =>
    {
      return new BeholderServiceInfo
      {
        ServiceName = "daemon",
        Version = "v1"
      };
    });

    private Task _psionixObserveTask;

    public BeholderDaemonWorker(
      IBeholderMqttClient mqttClient,
      BeholderOccipital occipital,
      IObserver<BeholderOccipitalEvent> occipitalObserver,
      BeholderPsionix psionix,
      IObserver<BeholderPsionixEvent> psionixObserver,
      BeholderEye eye,
      BeholderEyeObserver eyeObserver,
      ILogger<BeholderDaemonWorker> logger
    )
    {
      _mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));

      _occipital = occipital ?? throw new ArgumentNullException(nameof(occipital));
      _occipitalObserver = occipitalObserver ?? throw new ArgumentNullException(nameof(occipitalObserver));

      _psionix = psionix ?? throw new ArgumentNullException(nameof(psionix));
      _psionixObserver = psionixObserver ?? throw new ArgumentNullException(nameof(psionixObserver));

      _eye = eye ?? throw new ArgumentNullException(nameof(eye));
      _eyeObserver = eyeObserver ?? throw new ArgumentNullException(nameof(eyeObserver));

      _logger = logger ?? throw new ArgumentNullException(nameof(logger)); ;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      await _mqttClient.StartAsync();

      _eye.Subscribe(_eyeObserver);
      _psionix.Subscribe(_psionixObserver);
      _occipital.Subscribe(_occipitalObserver);

      _psionixObserveTask = _psionix.Observe(stoppingToken);

      while (!stoppingToken.IsCancellationRequested)
      {
        // Perform updates on 
        await _mqttClient.MqttClient.PublishEventAsync(BeholderConsts.PubSubName, "beholder/ctaf", _serviceInfo.Value, cancellationToken: stoppingToken);

        _eyeObserver.Pulse();

        //_logger.LogInformation("Daemon Pulsed");
        await Task.Delay(5000, stoppingToken);
      }
    }
  }
}