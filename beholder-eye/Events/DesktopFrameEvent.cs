namespace beholder_eye
{
  /// <summary>
  /// Event that is produced when a new desktop frame becomes available
  /// </summary>
  public record DesktopFrameEvent : BeholderEyeEvent
  {
    public DesktopFrame DesktopFrame { get; set; }
  }
}