namespace beholder_occipital.Models
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public record ObjectDetectionRequest
  {
    public ObjectDetectionRequest()
    {
      ImagePreProcessors = new List<ImageProcessor>();
    }

    [JsonPropertyName("queryImagePrefrontalKey")]
    public string QueryImagePrefrontalKey
    {
      get;
      set;
    }

    [JsonPropertyName("targetImagePrefrontalKey")]
    public string TargetImagePrefrontalKey
    {
      get;
      set;
    }

    [JsonPropertyName("imagePreProcessors")]
    public IList<ImageProcessor> ImagePreProcessors
    {
      get;
      set;
    }

    [JsonPropertyName("matchMaskSettings")]
    public MatchMaskSettings MatchMaskSettings
    {
      get;
      set;
    }

    [JsonPropertyName("outputImagePrefrontalKey")]
    public string OutputImagePrefrontalKey
    {
      get;
      set;
    }

    /// <summary>
    /// If specified, stores the captured target with the indicated key prefix
    /// </summary>
    [JsonPropertyName("targetImagePrefrontalKeyPrefix")]
    public string TargetImagePrefrontalKeyPrefix
    {
      get;
      set;
    }
  }
}
