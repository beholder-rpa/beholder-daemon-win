namespace beholder_psionix
{
  using beholder_psionix.Hotkeys;

  /// <summary>
  /// Event that is produced when a watched process has changed
  /// </summary>
  public record HotKeyEvent : BeholderPsionixEvent
  {
    public HotKey HotKey { get; set; }
  }
}