namespace beholder_nest.Json
{
  using beholder_nest.Attributes;
  using beholder_nest.Utils;
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  /// <summary>
  /// Represents the <see cref="JsonConverter"/> used to convert to/from an abstract class
  /// </summary>
  /// <typeparam name="T">The type of the abstract class to convert to/from</typeparam>
  public class AbstractClassConverter<T>
      : JsonConverter<T>
  {

    /// <summary>
    /// Initializes a new <see cref="AbstractClassConverter{T}"/>
    /// </summary>
    /// <param name="jsonSerializerOptions">The current <see cref="JsonSerializerOptions"/></param>
    public AbstractClassConverter(JsonSerializerOptions jsonSerializerOptions)
    {
      JsonSerializerOptions = jsonSerializerOptions;
      DiscriminatorAttribute discriminatorAttribute = typeof(T).GetCustomAttribute<DiscriminatorAttribute>();
      if (discriminatorAttribute == null)
        throw new NullReferenceException($"Failed to find the required '{nameof(DiscriminatorAttribute)}'");
      DiscriminatorProperty = typeof(T).GetProperty(discriminatorAttribute.Property, BindingFlags.Default | BindingFlags.Public | BindingFlags.Instance);
      if (DiscriminatorProperty == null)
        throw new NullReferenceException($"Failed to find the specified discriminator property '{discriminatorAttribute.Property}' in type '{typeof(T).Name}'");
      TypeMappings = new Dictionary<string, Type>();
      foreach (Type derivedType in TypeCacheUtil.FindFilteredTypes($"nposm:json-polymorph:{typeof(T).Name}",
          (t) => t.IsClass && !t.IsAbstract && t.BaseType == typeof(T)))
      {
        DiscriminatorValueAttribute discriminatorValueAttribute = derivedType.GetCustomAttribute<DiscriminatorValueAttribute>();
        if (discriminatorValueAttribute == null)
          continue;
        string discriminatorValue = null;
        if (discriminatorValueAttribute.Value.GetType().IsEnum)
          discriminatorValue = EnumHelper.Stringify((Enum)discriminatorValueAttribute.Value, DiscriminatorProperty.PropertyType);
        else
          discriminatorValue = discriminatorValueAttribute.Value.ToString();
        TypeMappings.Add(discriminatorValue, derivedType);
      }
    }

    /// <summary>
    /// Gets the current <see cref="JsonSerializerOptions"/>
    /// </summary>
    protected JsonSerializerOptions JsonSerializerOptions { get; }

    /// <summary>
    /// Gets the discriminator <see cref="PropertyInfo"/> of the abstract type to convert
    /// </summary>
    protected PropertyInfo DiscriminatorProperty { get; }

    /// <summary>
    /// Gets an <see cref="Dictionary{TKey, TValue}"/> containing the mappings of the converted type's derived types
    /// </summary>
    protected Dictionary<string, Type> TypeMappings { get; }

    /// <inheritdoc/>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      if (reader.TokenType != JsonTokenType.StartObject)
        throw new JsonException("Start object token type expected");
      using JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
      string discriminatorPropertyName = JsonSerializerOptions?.PropertyNamingPolicy == null ? DiscriminatorProperty.Name : JsonSerializerOptions.PropertyNamingPolicy.ConvertName(DiscriminatorProperty.Name);
      if (!jsonDocument.RootElement.TryGetProperty(discriminatorPropertyName, out JsonElement discriminatorProperty))
        throw new JsonException($"Failed to find the required '{DiscriminatorProperty.Name}' discriminator property");
      string discriminatorValue = discriminatorProperty.GetString();
      if (!TypeMappings.TryGetValue(discriminatorValue, out Type derivedType))
        throw new JsonException($"Failed to find the derived type with the specified discriminator value '{discriminatorValue}'");
      string json = jsonDocument.RootElement.GetRawText();
      return (T)JsonSerializer.Deserialize(json, derivedType, JsonSerializerOptions);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
      JsonSerializer.Serialize(writer, (object)value, options);
    }

  }
}
