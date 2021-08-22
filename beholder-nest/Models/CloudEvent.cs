namespace beholder_nest.Models
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  public record CloudEvent : ICloudEvent
  {
    public CloudEvent()
    {
      Id = Guid.NewGuid().ToString();
      Time = DateTime.Now;
      DataContentType = "application/json";
      SpecVersion = "1.0";
      ExtensionAttributes = new Dictionary<string, object>();
    }

    [JsonPropertyName("data")]
    public object Data { get; set; }

    [JsonPropertyName("datacontenttype")]
    public string DataContentType { get; set; }

    [JsonPropertyName("dataschema")]
    public string DataSchema { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonPropertyName("specversion")]
    public string SpecVersion { get; set; }

    [JsonPropertyName("subject")]
    public string Subject { get; set; }

    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> ExtensionAttributes { get; set; }
  }
}