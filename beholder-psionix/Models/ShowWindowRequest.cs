namespace beholder_psionix.Models
{
  using System.Text.Json.Serialization;

  public record ShowWindowRequest
  {
    [JsonPropertyName("processName")]
    public string ProcessName
    {
      get;
      set;
    }

    [JsonPropertyName("command")]
    public ShowWindowCommand Command
    {
      get;
      init;
    }
  }
}
