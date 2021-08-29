namespace beholder_eye
{
  using System.Drawing;

  /// <summary>
  /// Represents a capture of a screen region
  /// </summary>
  public record RegionCaptureEvent : BeholderEyeEvent
  {
    public string Name { get; set; }

    public byte[] Image { get; set; }

    public Rectangle RegionRectangle { get; set; }
  }
}
