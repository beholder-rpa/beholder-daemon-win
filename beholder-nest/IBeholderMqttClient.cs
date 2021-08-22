namespace beholder_nest
{
  using MQTTnet.Client;
  using System;
  using System.Threading;
  using System.Threading.Tasks;

  public interface IBeholderMqttClient : IObservable<MqttClientEvent>
  {
    /// <summary>
    /// Gets a value that indicates if the current instance is connected to MQTT
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets a value that indicates if the current instance has been disposed
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// Gets the underlying MqttClient implementation.
    /// </summary>
    IMqttClient MqttClient { get; }

    /// <summary>
    /// Start receiving messages from MQTT
    /// </summary>
    Task Connect(CancellationToken cancellationToken);

    /// <summary>
    /// Stops receiving messages from MQTT
    /// </summary>
    Task Disconnect();
  }
}