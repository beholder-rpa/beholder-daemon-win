namespace beholder_eye
{
  using System.Text.Json.Serialization;

  public record PointerImageStreamSettings
  {
    public PointerImageStreamSettings()
    {
      MaxFps = 0.5;
    }

    /// <summary>
    /// Indicates the maximum pointer images that will be sent per second. Defaults to 0.5
    /// </summary>
    [JsonPropertyName("maxFps")]
    public double? MaxFps
    {
      get;
      set;
    }
  }
}