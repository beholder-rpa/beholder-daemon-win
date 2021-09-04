﻿namespace beholder_psionix.Controllers
{
  using beholder_nest;
  using beholder_nest.Attributes;
  using beholder_psionix.Hotkeys;
  using Microsoft.Extensions.Logging;
  using MQTTnet;
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Threading.Tasks;

  [MqttController]
  public class HotKeyController
  {
    private readonly ILogger<HotKeyController> _logger;
    private readonly IBeholderMqttClient _beholderClient;

    public HotKeyController(ILogger<HotKeyController> logger, IBeholderMqttClient beholderClient)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _beholderClient = beholderClient ?? throw new ArgumentNullException(nameof(beholderClient));
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/hotkeys/register")]
    public Task RegisterHotKey(MqttApplicationMessage message)
    {
      var hotkeysString = Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
      if (HotKey.TryParse(hotkeysString, out var hotkeys))
      {
        foreach (var hotkey in hotkeys)
        {
          HotKeyManager.RegisterHotKey(hotkey);
          _logger.LogInformation($"Psionix HotKeys Registered {hotkey}");
        }
      }
      else
      {
        _logger.LogInformation($"Unable to parse Hotkeys {hotkeysString}");
      }

      return Task.CompletedTask;
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/hotkeys/unregister")]
    public Task UnregisterHotKey(MqttApplicationMessage message)
    {
      var hotkeysString = Encoding.UTF8.GetString(message.Payload, 0, message.Payload.Length);
      if (HotKey.TryParse(hotkeysString, out var hotkeys))
      {
        foreach (var hotkey in hotkeys)
        {
          HotKeyManager.UnregisterHotKey(hotkey);
          _logger.LogInformation($"Psionix HotKeys Unregistered {hotkey}");
        }
      }
      else
      {
        _logger.LogInformation($"Unable to parse Hotkeys {hotkeysString}");
      }

      return Task.CompletedTask;
    }

    [EventPattern("beholder/psionix/{HOSTNAME}/hotkeys/list_registered")]
    public async Task ListRegisteredHotKeys(MqttApplicationMessage message)
    {
      var registeredHotkeys = HotKeyManager.RegisteredHotKeys;
      var response = new List<string>();
      foreach (var hotKey in registeredHotkeys)
      {
        response.Add(hotKey.ToString());
      }

      await _beholderClient.PublishEventAsync($"beholder/psionix/{{HOSTNAME}}/hotkeys/registered_hotkeys", response);
    }
  }
}