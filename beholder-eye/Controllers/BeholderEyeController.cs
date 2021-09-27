namespace beholder_eye.Controllers
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
  public class BeholderEyeController
  {
    private readonly ILogger<BeholderEyeController> _logger;
    private readonly IBeholderMqttClient _beholderClient;
    private readonly BeholderEye _eye;
    private readonly BeholderEyeContext _context;

    public BeholderEyeController(ILogger<BeholderEyeController> logger, IBeholderMqttClient beholderClient, BeholderEye eye, BeholderEyeContext context)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _beholderClient = beholderClient ?? throw new ArgumentNullException(nameof(beholderClient));
      _eye = eye ?? throw new ArgumentNullException(nameof(eye));
      _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [EventPattern("beholder/eye/{HOSTNAME}/add_or_update_focus_region")]
    public Task AddOrUpdateFocusRegion(MqttApplicationMessage message)
    {
      // If the eye isn't currently observing, noop.
      if (_eye.Status == BeholderStatus.NotObserving)
      {
        _logger.LogTrace($"Eye not observing, skipping focus region update");
        return Task.CompletedTask;
      }

      var requestJson = Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
      var region = JsonSerializer.Deserialize<ObservationRegion>(requestJson);

      if (string.IsNullOrWhiteSpace(region.Name))
      {
        _logger.LogTrace($"A focus region update was requested, but a region name was not specified. Skipping.");
        return Task.CompletedTask;
      }

      _eye.AddOrUpdateFocusRegion(region.Name, region.BitmapSettings);

      _logger.LogInformation($"Eye Updated Region '{region.Name}' - X: {region.BitmapSettings?.X} Y: {region.BitmapSettings?.Y} Width: {region.BitmapSettings?.Width} Height: {region.BitmapSettings?.Height}");
      return Task.CompletedTask;
    }

    [EventPattern("beholder/eye/{HOSTNAME}/remove_focus_region")]
    public Task RemoveFocusRegion(MqttApplicationMessage message)
    {
      // If the eye isn't currently observing, noop.
      if (_eye.Status == BeholderStatus.NotObserving)
      {
        return Task.CompletedTask;
      }

      var regionNameString = Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
      _eye.RemoveFocusRegion(regionNameString, "Requested");

      _logger.LogInformation($"Eye Removed Region: {regionNameString}");
      return Task.CompletedTask;
    }

    [EventPattern("beholder/eye/{HOSTNAME}/report_status")]
    public async Task ReportStatus(MqttApplicationMessage message)
    {
      var beholderEyeInfo = new BeholderEyeInfo
      {
        Status = _eye.Status
      };

      await _beholderClient.PublishEventAsync(
        $"beholder/eye/{{HOSTNAME}}/status",
        beholderEyeInfo
      );
      _logger.LogInformation($"Eye provided status: {beholderEyeInfo}");
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

      _context.StopObserver();

      await ReportStatus(message);
    }
  }
}