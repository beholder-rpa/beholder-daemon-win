namespace beholder_psionix.Hotkeys
{
  using System;

  public class HotKeyEventArgs : EventArgs
  {
    public HotKeyEventArgs(HotKey hotkey)
    {
      Hotkey = hotkey;
    }

    public HotKeyEventArgs(IntPtr hotKeyParam)
    {
      uint param = (uint)hotKeyParam.ToInt64();
      Hotkey = new HotKey
      {
        Key = (System.Windows.Forms.Keys)((param & 0xffff0000) >> 16),
        Modifiers = (KeyModifiers)(param & 0x0000ffff)
      };
    }

    public HotKey Hotkey
    {
      get;
    }

  }
}