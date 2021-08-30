namespace beholder_occipital.Models
{
  using System.Text.Json.Serialization;

  public record ObjectDetectionRequest
  {
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
  }
}
