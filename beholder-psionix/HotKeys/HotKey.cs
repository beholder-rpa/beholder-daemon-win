namespace beholder_psionix.Hotkeys
{
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Text.RegularExpressions;
  using System.Windows.Forms;

  public record HotKey
  {
    //Key Regex, based loosely off of https://www.autohotkey.com/docs/commands/Send.htm#keynames
    private static readonly Regex KeysRegex = new Regex(
        @"(?:(?<![\{])(?<Modifiers>[!+^#]*?)(?<Key>[^{!+^#])(?![^{!+^#]*?[\}])|(?:(?<KeyNameModifiers>[!+^#]*?)\{(?<KeyName>[^}]+?)\}))"
        , RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

    public Keys Key
    {
      get;
      set;
    }

    public KeyModifiers Modifiers
    {
      get;
      set;
    }

    public override string ToString()
    {
      var result = new StringBuilder();
      if (Modifiers.HasFlag(KeyModifiers.Alt))
      {
        result.Append('!');
      }

      if (Modifiers.HasFlag(KeyModifiers.Control))
      {
        result.Append('^');
      }

      if (Modifiers.HasFlag(KeyModifiers.Shift))
      {
        result.Append('+');
      }

      if (Modifiers.HasFlag(KeyModifiers.Windows))
      {
        result.Append('#');
      }

      KeysConverter keysConverter = new KeysConverter();
      result.Append($"{{{keysConverter.ConvertToString(Key)}}}");

      return result.ToString();
    }

    public static bool TryParse(string keys, out IList<HotKey> hotkeys)
    {
      static KeyModifiers GetModifiers(Match m, string groupName = "Modifiers")
      {
        var modifiers = KeyModifiers.None;
        if (m.Groups.ContainsKey(groupName))
        {
          foreach (var c in m.Groups[groupName].Value)
          {
            switch (c)
            {
              case '!':
                modifiers |= KeyModifiers.Alt;
                break;
              case '^':
                modifiers |= KeyModifiers.Control;
                break;
              case '+':
                modifiers |= KeyModifiers.Shift;
                break;
              case '#':
                modifiers |= KeyModifiers.Windows;
                break;
            }
          }
        }

        return modifiers;
      }

      KeysConverter keysConverter = new KeysConverter();
      hotkeys = new List<HotKey>();
      foreach (Match match in KeysRegex.Matches(keys))
      {
        HotKey hotkey = null;
        // This must be null or empty as whitespace is valid.
        if (match.Groups.ContainsKey("Key") && !string.IsNullOrEmpty(match.Groups["Key"].Value))
        {
          var key = match.Groups["Key"].Value;

          try
          {
            hotkey = new HotKey()
            {
              Key = (Keys)keysConverter.ConvertFromInvariantString(key),
            };
            hotkey.Modifiers = GetModifiers(match);
          }
          catch (Exception)
          {
            hotkeys = null;
            return false;
          }


        }
        else if (match.Groups.ContainsKey("KeyName") && !string.IsNullOrWhiteSpace(match.Groups["KeyName"].Value))
        {
          var keyName = match.Groups["KeyName"].Value;

          try
          {
            hotkey = new HotKey()
            {
              Key = (Keys)keysConverter.ConvertFromInvariantString(keyName),
            };
            hotkey.Modifiers = GetModifiers(match, groupName: "KeyNameModifiers");
          }
          catch (Exception)
          {
            hotkeys = null;
            return false;
          }
        }

        if (hotkey != null)
        {
          hotkeys.Add(hotkey);
        }
      }

      return true;
    }
  }
}