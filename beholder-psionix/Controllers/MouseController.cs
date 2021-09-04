﻿namespace beholder_psionix.Controllers
{
  using beholder_nest;
  using beholder_nest.Attributes;
  using beholder_nest.Models;
  using beholder_psionix.Models;
  using Microsoft.Extensions.Logging;
  using System;
  using System.Threading.Tasks;

  [MqttController]
  public class MouseController
  {
    private readonly ILogger<MouseController> _logger;
    private readonly IBeholderMqttClient _beholderClient;

    public MouseController(ILogger<MouseController> logger, IBeholderMqttClient beholderClient)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _beholderClient = beholderClient ?? throw new ArgumentNullException(nameof(beholderClient));
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/mouse/set_speed")]
    public Task SetSpeed(ICloudEvent<int> message)
    {
      var newSpeed = message.Data;
      if (newSpeed < 1 || newSpeed > 20)
      {
        _logger.LogWarning($"The specified new speed is out of the range of valid values (1-20): {message.Data}");
        return Task.CompletedTask;
      }

      NativeMethods.SetMouseSpeed(newSpeed);
      return Task.CompletedTask;
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/mouse/set_mouse_info")]
    public Task SetMouseInfo(ICloudEvent<MouseInfo> message)
    {
      NativeMethods.SetMouseInfo(message.Data);
      return Task.CompletedTask;
    }
  }
}
