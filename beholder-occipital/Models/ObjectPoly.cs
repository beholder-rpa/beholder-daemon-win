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
  }
}
