namespace beholder_psionix
{
  using System;
  using System.Diagnostics;
  using System.Runtime.InteropServices;

  /// <summary>
  /// A hook that intercepts keyboard events.
  /// </summary>
  public sealed class KeyboardHook : IDisposable
  {
    private readonly IntPtr _hookPtr;

    public event EventHandler<Keys> OnKey;

    /// <summary>
    /// Creates a low-level keyboard hook and hooks it.
    /// </summary>
    public KeyboardHook()
    {
      _hookPtr = SetHook(HookCallback);
      var handle = GetType().Module.ModuleHandle;
    }

    private static IntPtr SetHook(HookCallback callback)
    {
      using var curProcess = Process.GetCurrentProcess();
      using var curModule = curProcess.MainModule;
      return NativeMethods.SetWindowsHookEx(HookType.WH_KEYBOARD_LL, callback, NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
    }

    private int HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
      if (nCode == NativeMethods.HC_ACTION)
      {
        KBDLLHOOKSTRUCT llh = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
        int msg = (int)wParam;

        OnKey?.Invoke(this, (Keys)llh.vkCode);
      }

      return NativeMethods.CallNextHookEx(_hookPtr, nCode, wParam, lParam);
    }

    #region IDisposable Support
    private bool _disposedValue = false; // To detect redundant calls

    void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          NativeMethods.UnhookWindowsHookEx(_hookPtr);
        }

        _disposedValue = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }
}