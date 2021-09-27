namespace beholder_occipital.Models
{
  using System.Text.Json.Serialization;

  public record ObjectDetectionOutputSettings
  {
    /// <summary>
    /// If specified, stores the OpenCV generated feature match to the file system.
    /// </summary>
    [JsonPropertyName("drawMatchesFilePrefix")]
    public string DrawMatchesFilePrefix
    {
      get;
      set;
    }

    /// <summary>
    /// If specified, stores the detected region as a coco dataset
    /// </summary>
    /// <remarks>
    /// The dataset that is saved is a single-file dataset 
    /// </remarks>
    [JsonPropertyName("drawMatchesFilePrefix")]
    public string CocoDatasetFilePrefix
    {
      get;
      set;
    }
  }
}
