namespace beholder_psionix
{
  using System;
  using System.Runtime.InteropServices;
  using System.Text;

  public static partial class NativeMethods
  {
    /// <summary>
    ///     The CallNextHookEx function passes the hook information to the next hook procedure in the current hook chain.
    ///     A hook procedure can call this function either before or after processing the hook information.
    /// </summary>
    /// <param name="idHook">This parameter is ignored.</param>
    /// <param name="nCode">[in] Specifies the hook code passed to the current hook procedure.</param>
    /// <param name="wParam">[in] Specifies the wParam value passed to the current hook procedure.</param>
    /// <param name="lParam">[in] Specifies the lParam value passed to the current hook procedure.</param>
    /// <returns>This value is returned by the next hook procedure in the chain.</returns>
    /// <remarks>
    ///     http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/setwindowshookex.asp
    /// </remarks>
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    internal static extern int CallNextHookEx(
        IntPtr idHook,
        int nCode,
        IntPtr wParam,
        IntPtr lParam);

    /// <summary>
    ///     The SetWindowsHookEx function installs an application-defined hook procedure into a hook chain.
    ///     You would install a hook procedure to monitor the system for certain types of events. These events
    ///     are associated either with a specific thread or with all threads in the same desktop as the calling thread.
    /// </summary>
    /// <param name="idHook">
    ///     [in] Specifies the type of hook procedure to be installed. This parameter can be one of the following values.
    /// </param>
    /// <param name="lpfn">
    ///     [in] Pointer to the hook procedure. If the dwThreadId parameter is zero or specifies the identifier of a
    ///     thread created by a different process, the lpfn parameter must point to a hook procedure in a dynamic-link
    ///     library (DLL). Otherwise, lpfn can point to a hook procedure in the code associated with the current process.
    /// </param>
    /// <param name="hMod">
    ///     [in] Handle to the DLL containing the hook procedure pointed to by the lpfn parameter.
    ///     The hMod parameter must be set to NULL if the dwThreadId parameter specifies a thread created by
    ///     the current process and if the hook procedure is within the code associated with the current process.
    /// </param>
    /// <param name="dwThreadId">
    ///     [in] Specifies the identifier of the thread with which the hook procedure is to be associated.
    ///     If this parameter is zero, the hook procedure is associated with all existing threads running in the
    ///     same desktop as the calling thread.
    /// </param>
    /// <returns>
    ///     If the function succeeds, the return value is the handle to the hook procedure.
    ///     If the function fails, the return value is NULL. To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    ///     http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/setwindowshookex.asp
    /// </remarks>
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    internal static extern IntPtr SetWindowsHookEx(
        HookType idHook,
        HookCallback callback,
        IntPtr hMod,
        uint dwThreadId);

    /// <summary>
    ///     The UnhookWindowsHookEx function removes a hook procedure installed in a hook chain by the SetWindowsHookEx
    ///     function.
    /// </summary>
    /// <param name="idHook">
    ///     [in] Handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to
    ///     SetWindowsHookEx.
    /// </param>
    /// <returns>
    ///     If the function succeeds, the return value is nonzero.
    ///     If the function fails, the return value is zero. To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    ///     http://msdn.microsoft.com/library/default.asp?url=/library/en-us/winui/winui/windowsuserinterface/windowing/hooks/hookreference/hookfunctions/setwindowshookex.asp
    /// </remarks>
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
    internal static extern bool UnhookWindowsHookEx(IntPtr idHook);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("user32.dll")]
    public static extern int GetKeyboardState(byte[] lpKeyState);

    [DllImport("user32.dll")]
    public static extern short GetKeyState(short nVirtKey);

    [DllImport("user32.dll", EntryPoint = "keybd_event")]
    public static extern void KeyboardEvent(byte bVk, byte bScan, uint dwFlags,
       UIntPtr dwExtraInfo);

    [DllImport("user32.dll", EntryPoint = "mouse_event")]
    public static extern void MouseEvent(uint dwFlags, uint dx, uint dy, uint dwData,
       UIntPtr dwExtraInfo);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetKeyNameText(int lParam, [Out] StringBuilder lpString,
       int nSize);

    [DllImport("user32.dll")]
    public static extern int MapVirtualKey(int uCode, int uMapType);

    [DllImport("user32.dll")]
    public static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    public static extern int ToUnicode(int wVirtKey, int wScanCode, byte[] lpKeyState,
       [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder pwszBuff, int cchBuff,
       uint wFlags);

    internal const int HC_ACTION = 0,
        HC_GETNEXT = 1,
        HC_SKIP = 2,
        HC_NOREMOVE = 3,
        HC_SYSMODALON = 4,
        HC_SYSMODALOFF = 5;

    internal const int KEYEVENTF_KEYUP = 0x2;
    internal const int WM_KEYDOWN = 0x100,
        WM_KEYUP = 0x101,
        WM_SYSKEYDOWN = 0x104,
        WM_SYSKEYUP = 0x105;

    internal const int WM_MOUSEMOVE = 0x200;
    internal const int WM_LBUTTONDOWN = 0x201;
    internal const int WM_LBUTTONUP = 0x202;
    internal const int WM_LBUTTONDBLCLK = 0x203;
    internal const int WM_RBUTTONDOWN = 0x204;
    internal const int WM_RBUTTONUP = 0x205;
    internal const int WM_RBUTTONDBLCLK = 0x206;
    internal const int WM_MBUTTONDOWN = 0x207;
    internal const int WM_MBUTTONUP = 0x208;
    internal const int WM_MBUTTONDBLCLK = 0x209;
    internal const int WM_MOUSEWHEEL = 0x20A;
    internal const int WM_MOUSEHWHEEL = 0x020E;
  }

  [StructLayout(LayoutKind.Sequential)]
  public class KBDLLHOOKSTRUCT
  {
    public int vkCode;
    public int scanCode;
    public int flags;
    public int time;
    public IntPtr dwExtraInfo;
  }

  [StructLayout(LayoutKind.Sequential)]
  public class MSLLHOOKSTRUCT
  {
    public Point pt;
    public int mouseData;
    public int flags;
    public int time;
    public IntPtr dwExtraInfo;
  }

}