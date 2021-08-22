namespace beholder_psionix
{
  using System.Runtime.InteropServices;

  /// <summary>
  /// Wrapper around the Winapi POINT type.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct Point
  {
    /// <summary>
    /// The X Coordinate.
    /// </summary>
    public int X;

    /// <summary>
    /// The Y Coordinate.
    /// </summary>
    public int Y;

    /// <summary>
    /// Creates a new POINT.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public Point(int x, int y)
    {
      X = x;
      Y = y;
    }

    /// <summary>
    /// Implicit cast.
    /// </summary>
    /// <returns></returns>
    public static implicit operator System.Drawing.Point(Point p)
    {
      return new System.Drawing.Point(p.X, p.Y);
    }

    /// <summary>
    /// Implicit cast.
    /// </summary>
    /// <returns></returns>
    public static implicit operator Point(System.Drawing.Point p)
    {
      return new Point(p.X, p.Y);
    }
  }
}