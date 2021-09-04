﻿namespace beholder_psionix
{
  using beholder_nest;
  using beholder_nest.Extensions;
  using beholder_psionix.Hotkeys;
  using beholder_psionix.Models;
  using Microsoft.Extensions.Logging;
  using System;
  using System.Text;
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
        case HotKeyEvent hotKeyEvent:
          HandleHotKey(hotKeyEvent.HotKey).Forget();
          break;
        case ActiveProcessChangedEvent activeProcessChangedEvent:
          HandleActiveProcessChanged(activeProcessChangedEvent.ProcessInfo).Forget();
          break;
        case ProcessChangedEvent processChangedEvent:
          HandleProcessChanged(processChangedEvent.ProcessInfo).Forget();
          break;
        case PointerPositionChangedEvent pointerPositionChangedEvent:
          HandlePointerPositionChanged(pointerPositionChangedEvent.PointerPosition).Forget();
          break;
        default:
          _logger.LogWarning($"Unhandled or unknown BeholderPsionixEvent: {psionixEvent}");
          break;
      }
    }

    public void Pulse()
    {
      var sysInfo = new SysInfo();

      _beholderClient
         .PublishEventAsync(
           $"beholder/psionix/{{HOSTNAME}}/system_information",
           sysInfo
         ).Forget();
    }

    private async Task HandleHotKey(HotKey hotKey)
    {
      var hotKeyBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(hotKey.ToString()));
      await _beholderClient.PublishEventAsync(
        $"beholder/psionix/{{HOSTNAME}}/hotkeys/pressed/{hotKeyBase64}",
        hotKey.ToString()
      );
      _logger.LogInformation($"Psionix registered hotkey was pressed: {hotKey}");
    }

    private async Task HandleActiveProcessChanged(ProcessInfo processInfo)
    {
      await _beholderClient.PublishEventAsync(
        $"beholder/psionix/{{HOSTNAME}}/active_process_changed",
        processInfo
      );
    }

    private async Task HandleProcessChanged(ProcessInfo processInfo)
    {
      await _beholderClient.PublishEventAsync(
        $"beholder/psionix/{{HOSTNAME}}/process_changed/{processInfo.ProcessName}",
        processInfo
      );
      _logger.LogInformation($"Psionix process changed: {processInfo}");
    }

    private async Task HandlePointerPositionChanged(PointerPosition pointerPosition)
    {
      await _beholderClient
        .PublishEventAsync(
          $"beholder/psionix/{{HOSTNAME}}/pointer_position",
          pointerPosition
        );
    }
  }
}