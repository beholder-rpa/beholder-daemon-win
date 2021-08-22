namespace beholder_nest.Mqtt
{
  using MQTTnet;
  using MQTTnet.Server;
  using System;

  public interface IMqttService
  {
    IApplicationMessagePublisher Publisher { get; }

    void ConfigureMqttServer(IMqttServer server);

    void SubscribeMessageRecieved(string topic, Action<MqttApplicationMessage> handler);
  }
}