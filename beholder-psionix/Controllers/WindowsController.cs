namespace beholder_psionix.Controllers
{
  using beholder_nest;
  using beholder_nest.Attributes;
  using beholder_nest.Models;
  using beholder_psionix.Models;
  using Microsoft.Extensions.Logging;
  using System;

  [MqttController]
  public class WindowsController
  {
    private readonly BeholderPsionix _psionix;
    private readonly ILogger<MouseController> _logger;
    private readonly IBeholderMqttClient _beholderClient;

    public WindowsController(BeholderPsionix psionix, ILogger<MouseController> logger, IBeholderMqttClient beholderClient)
    {
      _psionix = psionix ?? throw new ArgumentNullException(nameof(psionix));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _beholderClient = beholderClient ?? throw new ArgumentNullException(nameof(beholderClient));
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/ensure_foreground_window")]
    public void EnsureForegroundWindow(ICloudEvent<string> message)
    {
      var targetProcessName = message.Data;
      _psionix.EnsureForegroundWindow(targetProcessName);
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/move_window")]
    public void MoveWindow(ICloudEvent<MoveWindowRequest> message)
    {
      var request = message.Data;
      _psionix.MoveWindow(request);
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/show_window")]
    public void ShowWindow(ICloudEvent<ShowWindowRequest> message)
    {
      var request = message.Data;
      _psionix.ShowWindow(request);
    }
  }
}
