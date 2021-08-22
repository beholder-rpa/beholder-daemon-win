namespace beholder_eye
{
  using System.Text.Json.Serialization;

  public record MatrixPixelLocation
  {
    [JsonPropertyName("index")]
    public int Index
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