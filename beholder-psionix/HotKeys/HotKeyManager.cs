namespace beholder_psionix.Hotkeys
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.InteropServices;
  using System.Threading;
  using System.Windows.Forms;

  /// <summary>
  /// Manages Hotkeys
  /// </summary>
  public static class HotKeyManager
  {
    private static Dictionary<HotKey, int> _registeredHotKeys = new Dictionary<HotKey, int>();

    /// <summary>
    /// Event that is raised when the system indicates that a registered hotkey is pressed.
    /// </summary>
    public static event EventHandler<HotKeyEventArgs> HotKeyPressed;

    public static IReadOnlyList<HotKey> RegisteredHotKeys
    {
      get
      {
        return _registeredHotKeys.Keys.ToList().AsReadOnly();
      }
    }

    /// <summary>
    /// Registers the indicated hotkey and raises the HotKeyPressed when the system indicates of the hotkey being presssed.
    /// </summary>
    /// <param name="hotkey"></param>
    /// <returns></returns>
    public static int RegisterHotKey(HotKey hotkey)
    {
      if (_registeredHotKeys.ContainsKey(hotkey))
      {
        return _registeredHotKeys[hotkey];
      }

      _windowReadyEvent.WaitOne();
      int id = Interlocked.Increment(ref _id);
      _wnd.Invoke(new RegisterHotKeyDelegate(RegisterHotKeyInternal), _hwnd, id, (uint)hotkey.Modifiers, (uint)hotkey.Key);

      _registeredHotKeys.Add(hotkey, id);
      return id;
    }

    /// <summary>
    /// Unregisters the indicated hotkey.
    /// </summary>
    /// <param name="hotkey"></param>
    public static void UnregisterHotKey(HotKey hotkey)
    {
      if (!_registeredHotKeys.ContainsKey(hotkey))
      {
        return;
      }

      _wnd.Invoke(new UnRegisterHotKeyDelegate(UnRegisterHotKeyInternal), _hwnd, _registeredHotKeys[hotkey]);


      _registeredHotKeys.Remove(hotkey);
    }

    delegate void RegisterHotKeyDelegate(IntPtr hwnd, int id, uint modifiers, uint key);
    delegate void UnRegisterHotKeyDelegate(IntPtr hwnd, int id);

    private static void RegisterHotKeyInternal(IntPtr hwnd, int id, uint modifiers, uint key)
    {
      RegisterHotKey(hwnd, id, modifiers, key);
    }

    private static void UnRegisterHotKeyInternal(IntPtr hwnd, int id)
    {
      UnregisterHotKey(_hwnd, id);
    }

    private static void OnHotKeyPressed(HotKeyEventArgs e)
    {
      if (HotKeyPressed != null)
      {
        HotKeyPressed(null, e);
      }
    }

    private static volatile MessageWindow _wnd;
    private static volatile IntPtr _hwnd;
    private static readonly ManualResetEvent _windowReadyEvent = new ManualResetEvent(false);
    static HotKeyManager()
    {
      Thread messageLoop = new Thread(delegate ()
      {
        Application.Run(new MessageWindow());
      })
      {
        Name = "MessageLoopThread",
        IsBackground = true
      };
      messageLoop.Start();
    }

    private class MessageWindow : Form
    {
      public MessageWindow()
      {
        _wnd = this;
        _hwnd = this.Handle;
        _windowReadyEvent.Set();
      }

      protected override void WndProc(ref Message m)
      {
        if (m.Msg == WM_HOTKEY)
        {
          HotKeyEventArgs e = new HotKeyEventArgs(m.LParam);
          OnHotKeyPressed(e);
        }

        base.WndProc(ref m);
      }

      protected override void SetVisibleCore(bool value)
      {
        // Ensure the window never becomes visible
        base.SetVisibleCore(false);
      }

      private const int WM_HOTKEY = 0x312;
    }

    [DllImport("user32", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private static int _id = 0;
  }
}