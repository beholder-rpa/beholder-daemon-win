﻿namespace beholder_eye
{
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public record ObservationRegion
  {
    [JsonPropertyName("name")]
    public string Name
    {
      get;
      set;
    }

    [JsonPropertyName("kind")]
    public ObservationRegionKind Kind
    {
      get;
      set;
    }

    [JsonPropertyName("matrixSettings")]
    public MatrixSettings MatrixSettings
    {
      get;
      set;
    }

    [JsonPropertyName("bitmapSettings")]
    public ImageSettings BitmapSettings
    {
      get;
      set;
    }

    [JsonExtensionData]
    public IDictionary<string, JsonElement> AdditionalData
    {
      get;
      set;
    }
  }
}