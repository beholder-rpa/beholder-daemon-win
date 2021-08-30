namespace beholder_psionix
{
  public record ActiveProcessChangedEvent : BeholderPsionixEvent
  {
    public ProcessInfo ProcessInfo { get; set; }
  }
}
