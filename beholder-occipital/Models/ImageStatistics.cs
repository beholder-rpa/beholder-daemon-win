namespace beholder_occipital.Models
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public record Histogram
  {
    [JsonPropertyName("mean")]
    public double Mean { get; init; }
  }

  public record Color
  {
    [JsonPropertyName("r")]
    public int Red { get; init; }
    
    [JsonPropertyName("g")]
    public int Green { get; init; }

    [JsonPropertyName("b")]
    public int Blue { get; init; }

  }

  public record ImageStatistics
  {
    [JsonPropertyName("blue")]
    public Histogram Blue { get; init; }

    [JsonPropertyName("green")]
    public Histogram Green { get; init; }

    [JsonPropertyName("red")]
    public Histogram Red { get; init; }

    [JsonPropertyName("dominantColors")]
    public IList<Color> DominantColors { get; init; }
  }
}
