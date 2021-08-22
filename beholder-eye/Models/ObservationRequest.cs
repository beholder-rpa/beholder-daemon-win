namespace beholder_eye
{
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  public class ObservationRequest
  {
    [JsonPropertyName("adapterIndex")]
    public int? AdapterIndex
    {
      get;
      set;
    }

    [JsonPropertyName("deviceIndex")]
    public int? DeviceIndex
    {
      get;
      set;
    }

    [JsonPropertyName("regions")]
    public IList<ObservationRegion> Regions
    {
      get;
      set;
    }

    [JsonPropertyName("streamDesktopThumbnail")]
    public bool? StreamDesktopThumbnail
    {
      get;
      set;
    }

    [JsonPropertyName("desktopThumbnailStreamSettings")]
    public DesktopThumbnailStreamSettings DesktopThumbnailStreamSettings
    {
      get;
      set;
    }

    /// <summary>
    /// Indicates if pointer position changed events will be raised.
    /// </summary>
    [JsonPropertyName("watchPointerPosition")]
    public bool? WatchPointerPosition
    {
      get;
      set;
    }

    /// <summary>
    /// Inicates if pointer observed events will be raised.
    /// </summary>
    [JsonPropertyName("streamPointerImage")]
    public bool? StreamPointerImage
    {
      get;
      set;
    }

    [JsonPropertyName("pointerImageStreamSettings")]
    public PointerImageStreamSettings PointerImageStreamSettings
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