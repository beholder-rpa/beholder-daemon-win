namespace beholder_eye
{
  /// <summary>
  /// Event that is produced when the desktop's dimensions change.
  /// </summary>
  public record DesktopResizedEvent : BeholderEyeEvent
  {
    public int Height { get; set; }

    public int Width { get; set; }
  }
}