namespace beholder_occipital.Models
{
  using System.Text.Json.Serialization;

  public record Point
  {
    [JsonPropertyName("x")]
    public int X
    {
      get;
      set;
    }

    [JsonPropertyName("y")]
    public int Y
    {
      get;
      set;
    }
  }
}
