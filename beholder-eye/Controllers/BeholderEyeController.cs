namespace beholder_eye.Controllers
{
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
  public class BeholderEyeController
  {
    private readonly ILogger<BeholderEyeController> _logger;
    private readonly IMqttService _mqttService;
    private readonly BeholderEye _eye;
    private readonly BeholderEyeContext _context;

    public BeholderEyeController(ILogger<BeholderEyeController> logger, IMqttService mqttService, BeholderEye eye, BeholderEyeContext context)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _mqttService = mqttService ?? throw new ArgumentNullException(nameof(mqttService));
      _eye = eye ?? throw new ArgumentNullException(nameof(eye));
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [EventPattern("beholder/eye/{HOSTNAME}/report_status")]
    public async Task ReportStatus(MqttApplicationMessage message)
    {
      var beholderEyeInfo = new BeholderEyeInfo();
      if (_context.Observer != null)
      {
        beholderEyeInfo.Status = BeholderStatus.Observing;
      }

      await _mqttService.Publisher.PublishAsync(
          new MqttApplicationMessageBuilder()
              .WithTopic($"beholder/eye/{Environment.MachineName}/status")
              .WithPayload(JsonSerializer.Serialize(beholderEyeInfo))
              .Build(),
          CancellationToken.None
      );
    }

    [EventPattern("beholder/eye/{HOSTNAME}/request_align")]
    public void RequestAlign(MqttApplicationMessage message)
    {
      var requestJson = Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
      var request = JsonSerializer.Deserialize<AlignRequest>(requestJson);

      _eye.AlignRequest = request;
    }

    [EventPattern("beholder/eye/{HOSTNAME}/start_observing")]
    public async Task StartObserving(MqttApplicationMessage message)
    {
      if (_context.Observer != null)
      {
        _logger.LogInformation($"Beholder Eye is already observing.");
        return;
      }

      try
      {
        var requestJson = Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
        var request = JsonSerializer.Deserialize<ObservationRequest>(requestJson);
        _context.ObserverCts = new CancellationTokenSource();
        _context.Observer = Task.Run(async () =>
        {
          await _eye.ObserveWithUnwaveringSight(request, null, _context.ObserverCts.Token);
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"An error occurred during a request to start observing: {ex.Message}");
      }

      await ReportStatus(message);
    }

    [EventPattern("beholder/eye/{HOSTNAME}/stop_observing")]
    public async Task StopObserving(MqttApplicationMessage message)
    {
      if (_context.Observer == null)
      {
        _logger.LogInformation($"Beholder Eye is not observing.");
        return;
      }

      _context.ObserverCts.Cancel();
      _context.ObserverCts.Dispose();
      _context.ObserverCts = null;
      _context.Observer = null;

      await ReportStatus(message);
    }
  }
}