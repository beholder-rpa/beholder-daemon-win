namespace beholder_eye
{
  using System.Collections.Generic;

  /// <summary>
  /// Event that is raised when a matrix alignment map becomes available.
  /// </summary>
  public record AlignmentMapEvent : BeholderEyeEvent
  {
    public IList<MatrixPixelLocation> MatrixPixelLocations { get; set; }
  }
}