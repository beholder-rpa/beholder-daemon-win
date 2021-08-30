namespace beholder_psionix
{
  using System.Text.Json.Serialization;

  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum BeholderStatus
  {
    NotObserving,
    Observing
  }
}