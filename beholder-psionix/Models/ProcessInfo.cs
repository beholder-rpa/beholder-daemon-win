namespace beholder_psionix
{
  using System.Text.Json.Serialization;

  public record ProcessInfo
  {
    [JsonPropertyName("exists")]
    public bool Exists
    {
      get;
      init;
    }

    [JsonPropertyName("id")]
    public int? Id
    {
      get;
      init;
    }

    [JsonPropertyName("processName")]
    public string ProcessName
    {
      get;
      init;
    }

    [JsonPropertyName("mainWindowTitle")]
    public string MainWindowTitle
    {
      get;
      init;
    }

    [JsonPropertyName("processStatus")]
    public ProcessStatus? ProcessStatus
    {
      get;
      init;
    }

    [JsonPropertyName("showCommand")]
    public ShowWindowCommand? ShowCommand
    {
      get;
      init;
    }

    [JsonPropertyName("windowPosition")]
    public WindowPosition WindowPosition
    {
      get;
      init;
    }
  }
}