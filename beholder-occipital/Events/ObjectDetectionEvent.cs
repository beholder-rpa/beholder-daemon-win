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

    public IList<ObjectPoly> Locations
    {
      get;
      set;
    }
  }
}
