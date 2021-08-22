namespace beholder_eye
{
  /// <summary>
  /// Event that is produced with a new matrix frame becomes available
  /// </summary>
  public record MatrixFrameEvent : BeholderEyeEvent
  {
    public MatrixFrame MatrixFrame { get; set; }
  }
}