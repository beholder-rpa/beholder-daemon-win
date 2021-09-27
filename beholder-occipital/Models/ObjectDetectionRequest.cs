namespace beholder_occipital.Models
{
  using beholder_nest.Attributes;
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  [Discriminator(nameof(Type))]
  public abstract record ObjectDetectionRequest
  {
    public ObjectDetectionRequest()
    {
      PreProcessors = new List<ImageProcessor>();
      Type = ObjectDetectionType.SiftFlann;
      OutputSettings = new ObjectDetectionOutputSettings();
    }

    /// <summary>
    /// Indicates the object detection type. Defaults to SiftFlann
    /// </summary>
    [JsonPropertyName("type")]
    public virtual ObjectDetectionType Type
    {
      get;
    }

    /// <summary>
    /// Specifies any image processors that run prior to image detection
    /// </summary>
    [JsonPropertyName("preProcessors")]
    public IList<ImageProcessor> PreProcessors
    {
      get;
      set;
    }


    /// <summary>
    /// If specified configures settings associated with storing artifacts associated with the detection process.
    /// </summary>
    [JsonPropertyName("outputSettings")]
    public ObjectDetectionOutputSettings OutputSettings
    {
      get;
      set;
    }
  }

  [DiscriminatorValue(ObjectDetectionType.SiftFlann)]
  public record SiftFlannObjectDetectionRequest : ObjectDetectionRequest
  {
    public override ObjectDetectionType Type { get => ObjectDetectionType.SiftFlann; }

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
  }
}
