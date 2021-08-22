namespace beholder_eye
{
  /// <summary>
  /// Event that is produced when a new pointer image becomes available.
  /// </summary>
  public record PointerImageEvent : BeholderEyeEvent
  {
    public string Key { get; set; }

    public byte[] Image { get; set; }
  }
}