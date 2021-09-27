namespace beholder_nest.Mqtt
{
  using Microsoft.Extensions.Logging;
  using MQTTnet;
  using MQTTnet.Client.Receiving;
  using MQTTnet.Server;
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  public class MqttService :
      IMqttServerClientConnectedHandler,
      IMqttServerClientDisconnectedHandler,
      IMqttServerClientSubscribedTopicHandler,
      IMqttApplicationMessageReceivedHandler,
      IMqttService
  {
    private readonly IDictionary<string, Action<MqttApplicationMessage>> _handlers = new Dictionary<string, Action<MqttApplicationMessage>>();
    private readonly ILogger<MqttService> _logger;
    private IMqttServer _mqtt;

    public MqttService(ILogger<MqttService> logger)
    {
      _logger = logger;
    }

    public IApplicationMessagePublisher Publisher
    {
      get { return _mqtt; }
    }

    public void ConfigureMqttServer(IMqttServer mqtt)
    {
      _mqtt = mqtt;
      mqtt.ClientConnectedHandler = this;
      mqtt.ClientDisconnectedHandler = this;
      mqtt.ClientSubscribedTopicHandler = this;

      mqtt.ApplicationMessageReceivedHandler = this;
    }

    public void SubscribeMessageRecieved(string topic, Action<MqttApplicationMessage> handler)
    {
      if (string.IsNullOrWhiteSpace(topic))
      {
        throw new ArgumentNullException(nameof(topic));
      }

      if (handler == null)
      {
        throw new ArgumentNullException(nameof(topic));
      }

      _handlers.Add(topic, handler);
    }

    public Task HandleClientConnectedAsync(MqttServerClientConnectedEventArgs eventArgs)
    {
      _logger.LogInformation("Client connected.");
      return Task.CompletedTask;
    }

    public Task HandleClientDisconnectedAsync(MqttServerClientDisconnectedEventArgs eventArgs)
    {
      _logger.LogInformation("Client disconnected.");
      return Task.CompletedTask;
    }

    public Task HandleClientSubscribedTopicAsync(MqttServerClientSubscribedTopicEventArgs eventArgs)
    {
      _logger.LogInformation($"Topic Subscribed: {eventArgs.TopicFilter}");
      return Task.CompletedTask;
    }

    public Task InterceptApplicationMessagePublishAsync(MqttApplicationMessageInterceptorContext context)
    {
      _logger.LogInformation("app message publish");
      return Task.CompletedTask;
    }

    public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
    {
      foreach (var handler in _handlers)
      {
        var pattern = handler.Key;
        if (eventArgs.ApplicationMessage.IsTopicMatch(pattern))
        {
          handler.Value.Invoke(eventArgs.ApplicationMessage);
        }
      }
      return Task.CompletedTask;
    }
  }
}