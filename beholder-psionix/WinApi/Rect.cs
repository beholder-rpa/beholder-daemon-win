namespace beholder_psionix
{
  using System.Runtime.InteropServices;

  /// <summary>
  /// Wrapper around the Winapi RECT type.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct Rect
  {
    public int Left { get; set; }
    public int Top { get; set; }
    public int Right { get; set; }
    public int Bottom { get; set; }

    /// <summary>
    /// Creates a new RECT.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="top"></param>
    /// <param name="right"></param>
    /// <param name="bottom"></param>
    public Rect(int left, int top, int right, int bottom)
    {
      Left = left;
      Top = top;
      Right = right;
      Bottom = bottom;
    }

    /// <summary>
    /// Implicit cast.
    /// </summary>
    /// <returns></returns>
    public static implicit operator System.Drawing.Rectangle(Rect r)
    {
      return new System.Drawing.Rectangle(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
    }

    /// <summary>
    /// Implicit cast.
    /// </summary>
    /// <returns></returns>
    public static implicit operator Rect(System.Drawing.Rectangle r)
    {
      return new Rect(r.X, r.Y, r.Width + r.X, r.Y + r.Height);
    }
  }
}
