namespace beholder_eye
{
  // <summary>
  /// Represents an abstract event that is produced by BeholderEye implementations.
  /// Use Pattern Matching to handle specific instances of this type.
  /// Can be either one of the following:
  /// <see cref="DesktopFrameEvent"/>,
  /// <see cref="DesktopResizedEvent"/>,
  /// <see cref="MatrixFrameEvent"/>,
  /// <see cref="PointerImageEvent"/>,
  /// <see cref="ThumbnailImageEvent"/>,
  /// <see cref="PointerPositionChangedEvent"/>,
  /// <see cref="AlignmentMapEvent"/>,
  /// </summary>
  public abstract record BeholderEyeEvent
  {
  }
}