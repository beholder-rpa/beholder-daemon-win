namespace beholder_psionix
{
  using System.Text.Json.Serialization;

  public record ProcessInfo
  {
    [JsonPropertyName("exists")]
    public bool Exists
    {
      get;
      set;
    }

    [JsonPropertyName("id")]
    public int? Id
    {
      get;
      set;
    }

    [JsonPropertyName("processName")]
    public string ProcessName
    {
      get;
      set;
    }

    [JsonPropertyName("mainWindowTitle")]
    public string MainWindowTitle
    {
      get;
      set;
    }

    [JsonPropertyName("workingSet64")]
    public long? WorkingSet64
    {
      get;
      set;
    }

    [JsonPropertyName("processStatus")]
    public ProcessStatus? ProcessStatus
    {
      get;
      set;
    }

    [JsonPropertyName("windowPlacement")]
    public WindowStatus? WindowStatus
    {
      get;
      set;
    }
  }
}