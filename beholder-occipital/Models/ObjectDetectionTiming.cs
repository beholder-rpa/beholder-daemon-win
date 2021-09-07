namespace beholder_occipital.Models
{
  public record ObjectDetectionTiming
  {
    public long OverallTime
    {
      get;
      set;
    }

    public long DecodeTime
    {
      get;
      set;
    }

    public long PreProcessingTime
    {
      get;
      set;
    }

    public long ObjectDetectionTime
    {
      get;
      set;
    }
  }
}
