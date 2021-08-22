namespace beholder_eye
{
  using System.Text.Json.Serialization;

  public class ThumbnailImage
  {
    [JsonPropertyName("key")]
    public string Key
    {
      get;
      set;
    }

    [JsonPropertyName("image")]
    public string Image
    {
      get;
      set;
    }
  }
}