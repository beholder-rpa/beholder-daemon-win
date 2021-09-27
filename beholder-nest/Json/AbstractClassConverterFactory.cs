namespace beholder_nest.Json
{
  using beholder_nest.Attributes;
  using System;
  using System.Collections.Concurrent;
  using System.Reflection;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  /// <summary>
  /// Represents the <see cref="JsonConverterFactory"/> used to create <see cref="AbstractClassConverter{T}"/>
  /// </summary>
  public class AbstractClassConverterFactory
      : JsonConverterFactory
  {

    /// <summary>
    /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> containing the mappings of types to their respective <see cref="JsonConverter"/>
    /// </summary>
    private static readonly ConcurrentDictionary<Type, JsonConverter> Converters = new();

    /// <summary>
    /// Initializes a new <see cref="AbstractClassConverterFactory"/>
    /// </summary>
    /// <param name="jsonSerializerOptions">The current <see cref="System.Text.Json.JsonSerializerOptions"/></param>
    public AbstractClassConverterFactory(JsonSerializerOptions jsonSerializerOptions)
    {
      JsonSerializerOptions = jsonSerializerOptions;
    }

    /// <summary>
    /// Gets the current <see cref="JsonSerializerOptions"/>
    /// </summary>
    protected JsonSerializerOptions JsonSerializerOptions { get; }

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
      return typeToConvert.IsClass && typeToConvert.IsAbstract && typeToConvert.IsDefined(typeof(DiscriminatorAttribute));
    }

    /// <inheritdoc/>
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
      if (!Converters.TryGetValue(typeToConvert, out JsonConverter converter))
      {
        Type converterType = typeof(AbstractClassConverter<>).MakeGenericType(typeToConvert);
        converter = (JsonConverter)Activator.CreateInstance(converterType, JsonSerializerOptions);
        Converters.TryAdd(typeToConvert, converter);
      }
      return converter;
    }

  }
}
