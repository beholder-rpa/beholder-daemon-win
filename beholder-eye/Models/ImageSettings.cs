namespace beholder_eye
{
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public record ImageSettings
  {
    public ImageSettings()
    {
      Kind = ImageCaptureKind.Continuous;
    }

    [JsonPropertyName("kind")]
    public ImageCaptureKind Kind
    {
      get;
      init;
    }

    [JsonPropertyName("x")]
    public int X
    {
      get;
      init;
    }

    [JsonPropertyName("y")]
    public int Y
    {
      get;
      init;
    }

    [JsonPropertyName("width")]
    public int Width
    {
      get;
      init;
    }

    [JsonPropertyName("height")]
    public int Height
    {
      get;
      init;
    }

    /// <summary>
    /// Indicates how often the region is captured. Ex: An update rate of 250 will only capture the region 4 times a second.
    /// </summary>
    [JsonPropertyName("updateRateMs")]
    public int? UpdateRateMs
    {
      get;
      init;
    }

    [JsonExtensionData]
    public IDictionary<string, JsonElement> AdditionalData
    {
      get;
      init;
    }
  }
}