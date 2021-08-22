namespace beholder_psionix
{
  /// <summary>
  /// Event that is produced when a watched process has changed
  /// </summary>
  public record ProcessChangedEvent : BeholderPsionixEvent
  {
    public ProcessInfo ProcessInfo { get; set; }
  }
}