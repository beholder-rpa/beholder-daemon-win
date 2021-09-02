namespace beholder_occipital.Models
{
  using System.Text.Json.Serialization;

  [JsonConverter(typeof(JsonStringEnumConverter))]
  public enum ProcessorKind
  {
    Scale,
  }
}
