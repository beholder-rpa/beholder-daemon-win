namespace beholder_eye
{
  using System.Text.Json.Serialization;

  public record RegionCaptureInfo
  {
    [JsonPropertyName("prefrontalImageKey")]
    public string PrefrontalImageKey
    {
      get;
      set;
    }

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

    [JsonPropertyName("width")]
    public int Width
    {
      get;
      set;
    }

    [JsonPropertyName("height")]
    public int Height
    {
      get;
      set;
    }
  }
}
