namespace beholder_eye
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Text;
  using System.Text.Json;
  using System.Text.Json.Serialization;

  /// <summary>
  /// Represents decoded matrix data 
  /// </summary>
  public class MatrixFrame
  {
    private const int LineLength = 80;

    public MatrixFrame()
    {
      FrameTime = DateTime.UtcNow;
    }

    [JsonPropertyName("id")]
    public int? FrameId
    {
      get;
      set;
    }

    [JsonPropertyName("m")]
    public IList<byte> Metadata
    {
      get;
      set;
    }

    [JsonPropertyName("ft")]
    public DateTime? FrameTime
    {
      get;
      set;
    }

    [JsonPropertyName("d")]
    public object Data
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

    public static MatrixFrame CreateMatrixFrame(byte[] rawData, MatrixSettings settings)
    {
      return CreateMatrixFrame(rawData, settings.Map, settings.FrameIdIndex ?? 0, settings.FrameMetadataIndex ?? 1, settings.DataFormat ?? DataMatrixFormat.Json);
    }

    public static MatrixFrame CreateMatrixFrame(byte[] rawData, IList<MatrixPixelLocation> map, int frameIdIndex = 0, int metadataIndex = 1, DataMatrixFormat format = DataMatrixFormat.Json)
    {
      if (rawData == null)
      {
        throw new ArgumentNullException(nameof(rawData));
      }

      if (map == null)
      {
        throw new ArgumentNullException(nameof(map));
      }

      // Returns a read-only span of the raw data without the frame id and metadata.
      ReadOnlySpan<byte> GetRawDataSpan()
      {
        ReadOnlySpan<byte> rawDataSpan;
        if (frameIdIndex == 0 && metadataIndex == 1)
        {
          rawDataSpan = new ReadOnlySpan<byte>(rawData, 6, rawData.Length - 6);
        }
        else
        {
          var tempData = new List<byte>(rawData);
          tempData.RemoveRange(frameIdIndex, 3);
          tempData.RemoveRange(metadataIndex, 3);
          rawDataSpan = new ReadOnlySpan<byte>(tempData.ToArray());
        }

        return rawDataSpan;
      }

      string GetDataAsHex()
      {
        var sb = new StringBuilder(map.Count * 3);
        for (int lineNumber = 0; lineNumber < Math.Ceiling((decimal)map.Count / LineLength); lineNumber++)
        {
          var bytes = map.Count - lineNumber * LineLength > LineLength ? LineLength : map.Count - lineNumber * LineLength;
          var line = new byte[bytes];
          Array.Copy(rawData, lineNumber * LineLength * 3, line, 0, bytes);
          sb.AppendLine(BitConverter.ToString(line).Replace("-", " ", StringComparison.OrdinalIgnoreCase));
        }

        return sb.ToString();
      }

      string GetDataAsTextGrid()
      {
        var rawDataSpan = GetRawDataSpan();
        var sb = new StringBuilder(map.Count * 3);
        for (int lineNumber = 0; lineNumber < Math.Ceiling((decimal)map.Count / LineLength); lineNumber++)
        {
          var slice = rawDataSpan.Slice(lineNumber * LineLength, map.Count - lineNumber * LineLength > LineLength ? LineLength : map.Count - lineNumber * LineLength);
          sb.AppendLine(Encoding.ASCII.GetString(slice));
        }
        return sb.ToString();
      }

      string GetDataAsText()
      {
        var rawDataSpan = GetRawDataSpan();
        var eol = rawDataSpan.IndexOf((byte)0x00);
        if (eol == -1)
        {
          return Encoding.ASCII.GetString(rawDataSpan);
        }
        else
        {
          var slice = rawDataSpan.Slice(0, eol);
          return Encoding.ASCII.GetString(slice);
        }
      }

      JsonElement GetDataAsJson()
      {
        var reader = new Utf8JsonReader(GetRawDataSpan());
        try
        {
          if (!JsonDocument.TryParseValue(ref reader, out JsonDocument jsonDoc))
          {
            // Couldn't deserialize, return default JsonElement
            return default;
          }

          try
          {
            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Undefined)
            {
              return default;
            }

            return jsonDoc.RootElement.Clone();
          }
          finally
          {
            if (jsonDoc != null)
            {
              jsonDoc.Dispose();
            }
          }
        }
        catch
        {
          return default;
        }

      }

      IList<MatrixEvent> GetDataAsMatrixEvents()
      {
        var reader = new Utf8JsonReader(GetRawDataSpan());
        try
        {
          if (!JsonDocument.TryParseValue(ref reader, out JsonDocument jsonDoc))
          {
            // Couldn't deserialize, return default JsonElement
            return default;
          }

          var bootTime = DateTime.Now.Subtract(TimeSpan.FromMilliseconds(Environment.TickCount));

          try
          {
            using var ms = new MemoryStream();
            using var writer = new Utf8JsonWriter(ms);
            {
              writer.WriteStartArray();

              foreach (var eventElement in jsonDoc.RootElement.EnumerateArray())
              {
                writer.WriteStartObject();

                var hasFt = false;
                foreach (var element in eventElement.EnumerateObject())
                {
                  if (element.Name == "ft")
                  {
                    hasFt = true;
                    var ftValue = element.Value.GetDouble();
                    writer.WritePropertyName("et");
                    writer.WriteStringValue(bootTime.Add(TimeSpan.FromSeconds(ftValue)));
                  }
                  else
                  {
                    element.WriteTo(writer);
                  }
                }

                if (!hasFt)
                {
                  writer.WritePropertyName("et");
                  writer.WriteStringValue(DateTime.UtcNow);
                }
                writer.WriteEndObject();
              }

              writer.WriteEndArray();
              writer.Flush();
            }

            ms.Seek(0, SeekOrigin.Begin);
            var finalReader = new Utf8JsonReader(ms.ToArray());
            return JsonSerializer.Deserialize<IList<MatrixEvent>>(ref finalReader);
          }
          catch
          {
            // Something went wrong, return default JsonElement
            return default;
          }
          finally
          {
            if (jsonDoc != null)
            {
              jsonDoc.Dispose();
            }
          }
        }
        catch
        {
          return default;
        }
      }

      var dataMatrix = new MatrixFrame();
      if (frameIdIndex > -1)
      {
        var ix = frameIdIndex * 3;
        dataMatrix.FrameId = rawData[ix * 3] + (rawData[ix + 1] * 255) + (rawData[ix + 2] * 255 * 255);
      }

      if (metadataIndex > -1)
      {
        var ix = metadataIndex * 3;
        var metadata = new byte[3];
        metadata[0] = rawData[ix];
        metadata[1] = rawData[ix + 1];
        metadata[2] = rawData[ix + 2];
        dataMatrix.Metadata = metadata;
      }

      switch (format)
      {
        case DataMatrixFormat.Json:
          dataMatrix.Data = GetDataAsJson();
          break;
        case DataMatrixFormat.MatrixEvents:
          dataMatrix.Data = GetDataAsMatrixEvents();
          break;
        case DataMatrixFormat.TextGrid:
          dataMatrix.Data = GetDataAsTextGrid();
          break;
        case DataMatrixFormat.Text:
          dataMatrix.Data = GetDataAsText();
          break;
        case DataMatrixFormat.Hex:
          dataMatrix.Data = GetDataAsHex();
          break;
        case DataMatrixFormat.Raw:
          dataMatrix.Data = rawData;
          break;
      }

      return dataMatrix;
    }
  }
}