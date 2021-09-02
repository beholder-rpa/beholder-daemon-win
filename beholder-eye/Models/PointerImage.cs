namespace beholder_eye
{
  using System.Text.Json.Serialization;

  public record PointerImage
  {
    [JsonPropertyName("key")]
    public string Key
    {
      get;
      init;
    }

    [JsonPropertyName("image")]
    public string Image
    {
      get;
      init;
    }
  }
}