namespace beholder_occipital.Models
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public record ObjectPoly
  {
    public ObjectPoly()
    {
      Points = new List<Point>();
    }

    [JsonPropertyName("points")]
    public IList<Point> Points
    {
      get;
      set;
    }

    [JsonPropertyName("imageCacheKey")]
    public string ImageCacheKey
    {
      get;
      set;
    }
  }
}
