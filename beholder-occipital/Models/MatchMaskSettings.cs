namespace beholder_occipital.Models
{
  using System.Text.Json.Serialization;

  public record MatchMaskSettings
  {
    [JsonPropertyName("ratioThreshold")]
    public float RatioThreshold
    {
      get;
      set;
    }

    [JsonPropertyName("scaleIncrement")]
    public float ScaleIncrement
    {
      get;
      set;
    }

    [JsonPropertyName("rotationBins")]
    public int RotationBins
    {
      get;
      set;
    }
  }
}
