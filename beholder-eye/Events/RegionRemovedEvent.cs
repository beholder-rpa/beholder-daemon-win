namespace beholder_eye
{
  public record RegionRemovedEvent : BeholderEyeEvent
  {
    public string RegionName { get; init; }

    public string Reason { get; init; }
  }
}
