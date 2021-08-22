namespace beholder_psionix.Controllers
{
  using beholder_nest;
  using beholder_nest.Attributes;
  using Microsoft.Extensions.Logging;
  using System;

  [MqttController]
  public class BeholderController
  {
    private readonly ILogger<BeholderController> _logger;
    private readonly IBeholderMqttClient _beholderClient;
    private readonly BeholderPsionix _psionix;

    public BeholderController(ILogger<BeholderController> logger, IBeholderMqttClient beholderClient, BeholderPsionix psionix)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _beholderClient = beholderClient ?? throw new ArgumentNullException(nameof(beholderClient));
      _psionix = psionix ?? throw new ArgumentNullException(nameof(psionix));
    }
  }
}