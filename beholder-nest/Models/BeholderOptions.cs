namespace beholder_nest.Models
{
  using System;
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  /// <summary>
  /// Represents Beholder Daemon options used by various areas of the framework.
  /// </summary>
  public record BeholderOptions
  {
    public BeholderOptions()
    {
      ApiKey = "1234";
      MqttBrokerUrl = "wss://beholder-01.local/nexus/mqtt";
      HostName = Environment.MachineName;
      Username = "";
      Password = "";
      KeepAlivePeriodMs = 10000; // Default: 10s
      CommunicationTimeoutMs = 10000; // Default: 10s
      ReconnectDelayMs = 5000; // Default: 5s
      WillDelayIntervalMs = 25000; // Default: 25s
    }

    /// <summary>
    /// Gets or sets the API to use when securing RESTful API methods. Defaults to 1234.
    /// </summary>
    [JsonPropertyName("apiKey")]
    public string ApiKey
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the url of the Mqtt Broker that the beholder daemon will connect to (Defaults to beholder-01.local:1883, the default address of beholder rpi)
    /// </summary>
    [JsonPropertyName("mqttBrokerUrl")]
    public string MqttBrokerUrl
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the hostname to use. Defaults to the current machine name.
    /// </summary>
    [JsonPropertyName("hostName")]
    public string HostName
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a username to use when connecting to the MQTT Broker. Defaults to empty.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a password to use when connecting to the MQTT Broker. Defaults to empty.
    /// </summary>
    [JsonPropertyName("password")]
    public string Password
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the keep-alive period in milliseconds of the maximum interval that is permitted to lapse between client control packets. Default: 10s
    /// </summary>
    [JsonPropertyName("keepAlivePeriodMs")]
    public int? KeepAlivePeriodMs
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the timeout period in milliseconds of the maximum interval that is permitted to lapse between recieving a control packet from the broker. Default: 10s
    /// </summary>
    [JsonPropertyName("communicationTimeoutMs")]
    public int? CommunicationTimeoutMs
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the delay in milliseconds before automatically reconnecting to the broker. Default: 15s
    /// </summary>
    [JsonPropertyName("reconnectDelayMs")]
    public uint? ReconnectDelayMs
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the delay in milliseconds that a LWT message will be sent. Default: 25s
    /// </summary>
    [JsonPropertyName("willDelayIntervalMs")]
    public uint? WillDelayIntervalMs
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets any additional properties not previously described.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object> ExtensionData { get; set; }
  }
}