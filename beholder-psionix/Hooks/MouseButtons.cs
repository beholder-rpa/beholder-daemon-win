namespace beholder_psionix
{
  using System;

  // <summary>Specifies constants that define which mouse button was pressed.</summary>
  [Flags]
  public enum MouseButtons
  {
    /// <summary>The left mouse button was pressed.</summary>
    Left = 1048576, // 0x00100000
    /// <summary>No mouse button was pressed.</summary>
    None = 0,
    /// <summary>The right mouse button was pressed.</summary>
    Right = 2097152, // 0x00200000
    /// <summary>The middle mouse button was pressed.</summary>
    Middle = 4194304, // 0x00400000
    /// <summary>The first XButton was pressed.</summary>
    XButton1 = 8388608, // 0x00800000
    /// <summary>The second XButton was pressed.</summary>
    XButton2 = 16777216, // 0x01000000
  }
}