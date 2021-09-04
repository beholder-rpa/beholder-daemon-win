namespace beholder_nest.Models
{
  using System.Text.Json.Serialization;

  public record Size
  {
    [JsonPropertyName("width")]
    public int Width
    {
      get;
      init;
    }

    [JsonPropertyName("height")]
    public int Height
    {
      get;
      init;
    }
  }
}
