namespace beholder_eye
{
  /// <summary>
  /// Represents a capture of a screen region
  /// </summary>
  public record RegionCaptureEvent : BeholderEyeEvent
  {
    public string Name { get; set; }

    public byte[] Image { get; set; }
  }
}
