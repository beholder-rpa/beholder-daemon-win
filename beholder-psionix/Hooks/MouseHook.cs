namespace beholder_psionix
{
  using System;
  using System.Diagnostics;
  using System.Runtime.InteropServices;

  /// <summary>
  /// A hook that intercepts mouse events
  /// </summary>
  public class MouseHook
  {
    private readonly IntPtr _hookPtr;

    public event EventHandler<MouseButtons> OnMouse;

    /// <summary>
    /// Creates a low-level keyboard hook and hooks it.
    /// </summary>
    public MouseHook()
    {
      _hookPtr = SetHook(HookCallback);
    }

    private static IntPtr SetHook(HookCallback callback)
    {
      using var curProcess = Process.GetCurrentProcess();
      using var curModule = curProcess.MainModule;
      return NativeMethods.SetWindowsHookEx(HookType.WH_MOUSE_LL, callback, NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
    }

    private int HookCallback(int code, IntPtr wParam, IntPtr lParam)
    {
      if (code == NativeMethods.HC_ACTION)
      {
        MSLLHOOKSTRUCT llh = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
        int msg = (int)wParam;

        OnMouse?.Invoke(this, (MouseButtons)llh.mouseData);
      }
      return 0;
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          NativeMethods.UnhookWindowsHookEx(this._hookPtr);
        }

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }
}