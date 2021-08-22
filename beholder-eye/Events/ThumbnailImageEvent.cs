namespace beholder_eye
{
  /// <summary>
  /// Event that is produced when a new thumbnail image becomes available.
  /// </summary>
  public record ThumbnailImageEvent : BeholderEyeEvent
  {
    public string Key { get; set; }

    public byte[] Image { get; set; }
  }
}