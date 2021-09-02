namespace beholder_psionix
{
  /// <summary>
  /// Event that is produced when the pointer position has changed.
  /// </summary>
  public record PointerPositionChangedEvent : BeholderPsionixEvent
  {
    public PointerPosition PointerPosition { get; set; }
  }
}