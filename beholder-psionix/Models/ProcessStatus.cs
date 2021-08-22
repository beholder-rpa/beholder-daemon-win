namespace beholder_psionix
{
  using System.Text.Json.Serialization;

  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum ProcessStatus
  {
    Unknown = 0,
    Active = 1,
    Running = 2
  }
}