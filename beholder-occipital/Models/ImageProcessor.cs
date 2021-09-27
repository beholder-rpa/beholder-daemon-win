namespace beholder_occipital.Models
{
  using beholder_nest.Attributes;
  using System.Text.Json.Serialization;

  [Discriminator(nameof(Type))]
  public abstract record ImageProcessor
  {
    [JsonPropertyName("type")]
    public virtual ImageProcessorType Type
    {
      get;
    }
  }

  [DiscriminatorValue(ImageProcessorType.Scale)]
  public record ScaleImageProcessor : ImageProcessor
  {
    public override ImageProcessorType Type { get => ImageProcessorType.Scale; }

    [JsonPropertyName("scaleFactor")]
    public float? ScaleFactor
    {
      get;
      set;
    }
  }
}
