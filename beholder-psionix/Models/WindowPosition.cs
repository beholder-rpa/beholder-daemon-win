namespace beholder_psionix
{
  using System.Text.Json.Serialization;

  public record WindowPosition
  {
    public WindowPosition()
    {
    }

    public WindowPosition(System.Drawing.Rectangle rect)
    {
      X = rect.X;
      Y = rect.Y;
      Width = rect.Width;
      Height = rect.Height;
    }

    [JsonPropertyName("x")]
    public int X { get; init; }
    [JsonPropertyName("y")]
    public int Y { get; init; }
    [JsonPropertyName("width")]
    public int Width { get; init; }
    [JsonPropertyName("height")]
    public int Height { get; init; }
  }
}
