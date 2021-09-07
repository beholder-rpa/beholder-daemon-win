namespace beholder_occipital
{
  using beholder_occipital.Models;
  using System.Collections.Generic;

  public record ObjectDetectionEvent : BeholderOccipitalEvent
  {
    public ObjectDetectionEvent()
    {
      Locations = new List<ObjectPoly>();
    }

    public string QueryImagePrefrontalKey
    {
      get;
      init;
    }

    public IList<ObjectPoly> Locations
    {
      get;
      init;
    }

    public ObjectDetectionTiming Timing
    {
      get;
      init;
    }
  }
}
