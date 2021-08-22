namespace beholder_psionix_tests
{
  using beholder_psionix.Hotkeys;
  using System.Windows.Forms;
  using Xunit;

  public class HotkeyTests
  {
    [Fact]
    public void ShouldParseSimpleKey()
    {
      var didParse = HotKey.TryParse("!{A}", out var result);
      Assert.True(didParse);
      Assert.True(result.Count == 1);
      Assert.Equal(Keys.A, result[0].Key);
      Assert.Equal(KeyModifiers.Alt, result[0].Modifiers);
    }

    [Fact]
    public void ShouldReturnFalseWhenCannotParse()
    {
      var didParse = HotKey.TryParse("!{ASDF}", out var result);
      Assert.False(didParse);
      Assert.True(result == null);
    }

    [Fact]
    public void ShouldParseKeysWithMultipleModifiers()
    {
      var didParse = HotKey.TryParse("^!+#{LWin}", out var result);
      Assert.True(didParse);
      Assert.True(result.Count == 1);
      Assert.Equal(Keys.LWin, result[0].Key);
      Assert.Equal(KeyModifiers.Alt | KeyModifiers.Shift | KeyModifiers.Control | KeyModifiers.Windows, result[0].Modifiers);
    }

    [Fact]
    public void ShouldParseMultipleKeys()
    {
      var didParse = HotKey.TryParse("^!+#{NumPad9}+{Space}", out var result);
      Assert.True(didParse);
      Assert.True(result.Count == 2);
      Assert.Equal(Keys.NumPad9, result[0].Key);
      Assert.Equal(KeyModifiers.Alt | KeyModifiers.Shift | KeyModifiers.Control | KeyModifiers.Windows, result[0].Modifiers);

      Assert.Equal(Keys.Space, result[1].Key);
      Assert.Equal(KeyModifiers.Shift, result[1].Modifiers);
    }

    [Fact]
    public void ShouldConvertToString()
    {
      var hotKeyString = "^!+#{NumPad9}+{Space}";
      var didParse = HotKey.TryParse(hotKeyString, out var result);

      Assert.True(didParse);
      Assert.Equal("!^+#{NumPad9}", result[0].ToString());
      Assert.Equal("+{Space}", result[1].ToString());
    }
  }
}