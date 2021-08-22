namespace beholder_psionix_tests
{
  using beholder_psionix.Hotkeys;
  using System;
  using System.Windows.Forms;
  using Xunit;

  public class HotkeyManagerTests
  {
    [Fact]
    public void ShouldRegisterHotkey()
    {
      HotKeyManager.HotKeyPressed += HotkeyManager_HotKeyPressed;

      HotKeyManager.RegisterHotKey(new HotKey() { Key = Keys.A, Modifiers = KeyModifiers.Alt });
      Console.ReadLine();
    }

    private void HotkeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
    {
      Console.WriteLine(e.Hotkey);
    }
  }
}