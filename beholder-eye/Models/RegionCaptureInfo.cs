namespace beholder_eye
{
  using System.Text.Json.Serialization;

  public record RegionCaptureInfo
  {
    [JsonPropertyName("prefrontalImageKey")]
    public string PrefrontalImageKey
    {
      get;
      init;
    }

    [JsonPropertyName("x")]
    public int X
    {
      get;
      init;
    }

    [JsonPropertyName("y")]
    public int Y
    {
      get;
      init;
    }

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