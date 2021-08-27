namespace beholder_psionix
{
  using beholder_nest;
  using beholder_nest.Extensions;
  using beholder_nest.Mqtt;
  using beholder_psionix.Hotkeys;
  using Microsoft.Extensions.Logging;
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using System.Timers;

  public sealed class BeholderPsionix : IObservable<BeholderPsionixEvent>, IDisposable
  {
    public const int DefaultProcessRefreshMs = 100;

    private readonly ConcurrentDictionary<string, (Timer timer, ProcessInfo lastProcessInfo)> _processObservers = new ConcurrentDictionary<string, (Timer timer, ProcessInfo lastProcessInfo)>();
    private readonly ConcurrentDictionary<IObserver<BeholderPsionixEvent>, BeholderPsionixEventUnsubscriber> _observers = new ConcurrentDictionary<IObserver<BeholderPsionixEvent>, BeholderPsionixEventUnsubscriber>();
    private readonly ILogger<BeholderPsionix> _logger;
    private readonly IBeholderMqttClient _beholderClient;

    public BeholderPsionix(ILogger<BeholderPsionix> logger, IBeholderMqttClient beholderClient)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _beholderClient = beholderClient ?? throw new ArgumentNullException(nameof(beholderClient));

      HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;
    }

    public bool IsDisposed
    {
      get;
      set;
    }

    public IList<ProcessInfo> GetProcesses()
    {
      var result = new List<ProcessInfo>();
      foreach (var process in Process.GetProcesses())
      {
        result.Add(GetProcessInfo(process, process.ProcessName));
      }
      return result;
    }

    /// <summary>
    /// Returns a ProcessInfo object that represents the current active process.
    /// </summary>
    /// <returns></returns>
    public ProcessInfo GetActiveProcess()
    {
      var foregroundWindowIntPtr = NativeMethods.GetForegroundWindow();
      var currentProcess = Process.GetProcesses()
          .FirstOrDefault(p => p.MainWindowHandle == foregroundWindowIntPtr);

      if (currentProcess == null)
      {
        return null;
      }

      return GetProcessInfo(currentProcess, currentProcess.ProcessName);
    }

    /// <summary>
    /// Returns a ProcessInfo object for the first process with the given name.
    /// </summary>
    /// <param name="processName"></param>
    /// <returns></returns>
    public ProcessInfo GetProcessInfo(string processName)
    {
      var process = Process.GetProcesses()
              .FirstOrDefault(p => p.ProcessName == processName);

      return GetProcessInfo(process, processName);
    }

    /// <summary>
    /// Returns a ProcessInfo object for the given process.
    /// </summary>
    /// <param name="process"></param>
    /// <returns></returns>
    public ProcessInfo GetProcessInfo(Process process, string processName)
    {
      if (process == null)
      {
        return new ProcessInfo()
        {
          Exists = false,
          ProcessName = processName
        };
      }

      var foregroundWindowIntPtr = NativeMethods.GetForegroundWindow();

      var processInfo = new ProcessInfo()
      {
        Exists = true,
        Id = process.Id,
        ProcessName = process.ProcessName,
        MainWindowTitle = process.MainWindowTitle,
        WorkingSet64 = process.WorkingSet64
      };

      if (process != null)
      {
        if (process.MainWindowHandle == foregroundWindowIntPtr)
        {
          processInfo.ProcessStatus = ProcessStatus.Active;
        }
        else
        {
          processInfo.ProcessStatus = ProcessStatus.Running;
        }
      }

      var placement = new WindowPlacement();
      if (NativeMethods.GetWindowPlacement(process.MainWindowHandle, ref placement))
      {
        switch (placement.showCmd)
        {
          case 1:
            processInfo.WindowStatus = WindowStatus.ShowNormal;
            break;
          case 2:
            processInfo.WindowStatus = WindowStatus.Minimize;
            break;
          case 3:
            processInfo.WindowStatus = WindowStatus.Maximized;
            break;
        }
      }

      return processInfo;
    }

    public ICollection<string> GetObservedProcesses()
    {
      return _processObservers.Keys;
    }

    /// <summary>
    /// Instruct Psionix to start raising ProcessChanged events when the specified process becomes active or is minimized/maximized.
    /// </summary>
    /// <param name="processName"></param>
    public void ObserveProcess(string processName)
    {

      if (_processObservers.ContainsKey(processName))
      {
        _logger.LogInformation($"Already observing Process '{processName}'.");
        return;
      }

      var newTimer = new Timer(DefaultProcessRefreshMs)
      {
        AutoReset = true,
        Enabled = true
      };

      newTimer.Elapsed += (sender, e) =>
      {
        if (_processObservers.TryGetValue(processName, out (Timer timer, ProcessInfo lastProcessInfo) state))
        {
          var processInfo = GetProcessInfo(processName);

          if (state.lastProcessInfo == null || state.lastProcessInfo != processInfo with { WorkingSet64 = state.lastProcessInfo.WorkingSet64 })
          {
            if (_processObservers.TryUpdate(processName, (state.timer, processInfo), state))
            {
              OnBeholderPsionixEvent(new ProcessChangedEvent() { ProcessInfo = processInfo });
            }
          }
        }
      };

      _processObservers.AddOrUpdate(processName, (key) => (newTimer, null), (key, oldState) => { oldState.timer.Stop(); oldState.timer.Dispose(); return (newTimer, null); });
    }

    /// <summary>
    /// Instruct Psionix to stop raising process changed events for the given process.
    /// </summary>
    /// <param name="processName"></param>
    public void IgnoreProcess(string processName)
    {
      if (_processObservers.ContainsKey(processName))
      {
        _logger.LogInformation($"Process '{processName}' is not being observed.");
        return;
      }

      if (_processObservers.TryRemove(processName, out (Timer timer, ProcessInfo lastProcessInfo) state))
      {
        state.timer.Stop();
        state.timer.Dispose();
      }
    }

    /// <summary>
    /// Activates the process with the given name is the active foreground window.
    /// </summary>
    /// <param name="processName"></param>
    public void EnsureForegroundWindow(string processName)
    {
      _logger.LogInformation($"Ensuring process '{processName}' is in the foreground...");

      var process = Process.GetProcesses()
          .FirstOrDefault(p => p.ProcessName == processName);

      if (process == null)
      {
        _logger.LogInformation($"Process '{processName}' is not currently running.");
        return;
      }

      // Activate the first application we find with this name
      var foregroundWindowIntPtr = NativeMethods.GetForegroundWindow();
      var currentProcess = Process.GetProcesses()
          .FirstOrDefault(p => p.MainWindowHandle == foregroundWindowIntPtr);
      Console.WriteLine($"{currentProcess.ProcessName} - {foregroundWindowIntPtr}");
      if (foregroundWindowIntPtr != process.MainWindowHandle)
      {
        NativeMethods.ShowWindow(process.MainWindowHandle, ShowWindowCommands.Minimize);
        NativeMethods.ShowWindow(process.MainWindowHandle, ShowWindowCommands.Restore);
        NativeMethods.SetForegroundWindow(process.MainWindowHandle);
      }

      _logger.LogInformation($"Process '{processName}' is now in the foreground.");
    }

    public IDisposable Subscribe(IObserver<BeholderPsionixEvent> observer)
    {
      return _observers.GetOrAdd(observer, new BeholderPsionixEventUnsubscriber(this, observer));
    }

    /// <summary>
    /// Produces Beholder Psionix Events
    /// </summary>
    /// <param name="beholderEyeEvent"></param>
    private void OnBeholderPsionixEvent(BeholderPsionixEvent beholderPsionixEvent)
    {
      Parallel.ForEach(_observers.Keys, (observer) =>
      {
        try
        {
          observer.OnNext(beholderPsionixEvent);
        }
        catch (Exception)
        {
          // Do Nothing.
        }
      });
    }

    private void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
    {
      var hotKeyBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(e.Hotkey.ToString()));
      _beholderClient.MqttClient.PublishEventAsync(BeholderConsts.PubSubName, $"beholder/psionix/{{HOSTNAME}}/hotkeys/pressed/{hotKeyBase64}", e.Hotkey.ToString()).Forget(); _logger.LogInformation($"Psionix registered hotkey was pressed: {e.Hotkey}");
    }

    #region IDisposable Support

    private void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          if (_processObservers != null)
          {
            foreach (var state in _processObservers)
            {
              state.Value.timer.Stop();
              state.Value.timer.Dispose();
            }
          }

          HotKeyManager.HotKeyPressed -= HotKeyManager_HotKeyPressed;
        }

        IsDisposed = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
    #endregion

    #region Nested Classes
    private sealed class BeholderPsionixEventUnsubscriber : IDisposable
    {
      private readonly BeholderPsionix _parent;
      private readonly IObserver<BeholderPsionixEvent> _observer;

      public BeholderPsionixEventUnsubscriber(BeholderPsionix parent, IObserver<BeholderPsionixEvent> observer)
      {
        _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        _observer = observer ?? throw new ArgumentNullException(nameof(observer));
      }

      public void Dispose()
      {
        if (_observer != null && _parent._observers.ContainsKey(_observer))
        {
          _parent._observers.TryRemove(_observer, out _);
        }
      }
    }
    #endregion
  }
}