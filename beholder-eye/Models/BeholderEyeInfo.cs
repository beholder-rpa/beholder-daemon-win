namespace beholder_eye
{
  using System.Text.Json.Serialization;

  public class BeholderEyeInfo
  {
    [JsonPropertyName("status")]
    public BeholderStatus Status
    {
      get;
      set;
    }
  }
}