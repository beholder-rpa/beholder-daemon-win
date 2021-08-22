namespace beholder_eye
{
  using System.Text.Json.Serialization;

  public record PointerPosition
  {
    [JsonPropertyName("x")]
    public int? X
    {
      get;
      internal set;
    }

    [JsonPropertyName("y")]
    public int? Y
    {
      get;
      internal set;
    }

    [JsonPropertyName("v")]
    public bool? Visible
    {
      get;
      internal set;
    }
  }
}