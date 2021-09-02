namespace beholder_psionix
{
  using System.Text.Json.Serialization;

  public record PointerPosition
  {
    [JsonPropertyName("x")]
    public int? X
    {
      get;
      init;
    }

    [JsonPropertyName("y")]
    public int? Y
    {
      get;
      init;
    }

    [JsonPropertyName("v")]
    public bool? Visible
    {
      get;
      init;
    }
  }
}