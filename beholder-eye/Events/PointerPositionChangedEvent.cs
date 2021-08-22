namespace beholder_eye
{
  /// <summary>
  /// Event that is produced when the pointer position has changed.
  /// </summary>
  public record PointerPositionChangedEvent : BeholderEyeEvent
  {
    public PointerPosition PointerPosition { get; set; }
  }
}