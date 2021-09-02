namespace beholder_psionix
{
  using beholder_psionix.Hotkeys;
  using Microsoft.Extensions.Logging;
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Threading;
  using System.Threading.Tasks;
  using Timer = System.Timers.Timer;

  /// <summary>
  /// Represents an observable object that encapsulates monitoring various system-related resources and notifies subscribers when events occur on those resources.
  /// </summary>
  public sealed class BeholderPsionix : IObservable<BeholderPsionixEvent>, IDisposable
  {
    public const int DefaultProcessRefreshMs = 100;

    private readonly ConcurrentDictionary<string, (Timer timer, ProcessInfo lastProcessInfo)> _processObservers = new ConcurrentDictionary<string, (Timer timer, ProcessInfo lastProcessInfo)>();
    private readonly ConcurrentDictionary<IObserver<BeholderPsionixEvent>, BeholderPsionixEventUnsubscriber> _observers = new ConcurrentDictionary<IObserver<BeholderPsionixEvent>, BeholderPsionixEventUnsubscriber>();
    private readonly ILogger<BeholderPsionix> _logger;

    private ProcessInfo _activeProcess;
    private PointerPosition _pointerPosition;

    public BeholderPsionix(ILogger<BeholderPsionix> logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;
    }

    public bool IsDisposed
    {
      get;
      set;
    }

    /// <summary>
    /// Gets a value that indicates the status of the Beholder Psionix
    /// </summary>
    public BeholderStatus Status
    {
      get;
      private set;
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
      var _ = NativeMethods.GetWindowThreadProcessId(foregroundWindowIntPtr, out int foregroundWindowPid);
      var currentProcess = Process.GetProcessById(foregroundWindowPid);

      if (currentProcess == null)
      {
        return null;
      }

      return GetProcessInfo(currentProcess, currentProcess.ProcessName);
    }

    public static PointerPosition GetPointerPosition()
    {
      var result = NativeMethods.GetCursorPos(out Point lpPoint);
      if (!result)
      {
        throw new InvalidOperationException("Error Obtaining CursorPos");
      }

      return new PointerPosition()
      {
        X = lpPoint.X,
        Y = lpPoint.Y,
        Visible = true,
      };
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

      var processStatus = ProcessStatus.Unknown;
      if (process != null)
      {
        if (process.MainWindowHandle == foregroundWindowIntPtr)
        {
          processStatus = ProcessStatus.Active;
        }
        else
        {
          processStatus = ProcessStatus.Running;
        }
      }

      var placement = new WindowPlacement();
      var windowStatus = WindowStatus.Hidden;
      if (NativeMethods.GetWindowPlacement(process.MainWindowHandle, ref placement))
      {
        switch (placement.showCmd)
        {
          case 1:
            windowStatus = WindowStatus.ShowNormal;
            break;
          case 2:
            windowStatus = WindowStatus.Minimize;
            break;
          case 3:
            windowStatus = WindowStatus.Maximized;
            break;
        }
      }

      Rect position = new Rect();
      NativeMethods.GetWindowRect(process.MainWindowHandle, ref position);

      return new ProcessInfo()
      {
        Exists = true,
        Id = process.Id,
        ProcessName = process.ProcessName,
        MainWindowTitle = process.MainWindowTitle,
        ProcessStatus = processStatus,
        WindowStatus = windowStatus,
        WindowPosition = new WindowPosition(position),
      };
    }

    public Task Observe(CancellationToken token)
    {
      if (Status == BeholderStatus.Observing)
      {
        throw new InvalidOperationException("Beholder's Psionix is already observing.");
      }

      // Observe the screen on a seperate thread.
      return Task.Factory.StartNew(() =>
      {
        Status = BeholderStatus.Observing;
        _logger.LogInformation("The indeterminable logic of the Beholder's mental acquity is now focused upon the system...");
        while(!token.IsCancellationRequested)
        {
          var currentActiveProcess = GetActiveProcess();
          if (currentActiveProcess != _activeProcess)
          {
            _activeProcess = currentActiveProcess;
            OnBeholderPsionixEvent(new ActiveProcessChangedEvent() { ProcessInfo = _activeProcess });
          }

          var currentPointerPosition = GetPointerPosition();
          if (currentPointerPosition != _pointerPosition)
          {
            _pointerPosition = currentPointerPosition;
            OnBeholderPsionixEvent(new PointerPositionChangedEvent() { PointerPosition = _pointerPosition });
          }
          Task.Delay(100);
        }

        Status = BeholderStatus.NotObserving;
        _logger.LogInformation("The Beholder's Psionix have focused its attention elsewhere.");
      }, token, TaskCreationOptions.None, TaskScheduler.Default);
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

          if (state.lastProcessInfo == null || state.lastProcessInfo != processInfo with { })
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
      OnBeholderPsionixEvent(new HotKeyEvent() { HotKey = e.Hotkey });
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