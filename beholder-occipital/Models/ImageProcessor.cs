namespace beholder_occipital.Models
{
  using System.Text.Json.Serialization;

  public record ImageProcessor
  {
    [JsonPropertyName("kind")]
    public ProcessorKind Kind
    {
      get;
      set;
    }

    [JsonPropertyName("scaleFactor")]
    public float? ScaleFactor
    {
      get;
      set;
    }
  }
}
