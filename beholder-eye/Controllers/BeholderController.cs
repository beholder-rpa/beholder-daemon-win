namespace beholder_eye.Controllers
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
    private readonly BeholderEye _eye;

    public BeholderController(ILogger<BeholderController> logger, IBeholderMqttClient beholderClient, BeholderEye eye)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _beholderClient = beholderClient ?? throw new ArgumentNullException(nameof(beholderClient));
      _eye = eye ?? throw new ArgumentNullException(nameof(eye));
    }
  }
}