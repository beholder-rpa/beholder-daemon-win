namespace beholder_psionix.Models
{
  using System.Text.Json.Serialization;

  public record MoveWindowRequest
  {
    [JsonPropertyName("processName")]
    public string ProcessName
    {
      get;
      set;
    }

    [JsonPropertyName("targetPosition")]
    public WindowPosition TargetPosition
    {
      get;
      init;
    }

    [JsonPropertyName("repaint")]
    public bool Repaint
    {
      get;
      init;
    }
  }
}
