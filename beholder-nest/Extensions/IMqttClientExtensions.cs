namespace beholder_nest.Mqtt
{
  using beholder_nest.Models;
  using beholder_nest.Routing;
  using MQTTnet;
  using MQTTnet.Client;
  using System.Collections.Generic;
  using System.Text.Json;
  using System.Threading;
  using System.Threading.Tasks;

  public static class IMqttClientExtensions
  {
    public static void SubscribeControllers(this IMqttClient client, MqttApplicationMessageRouter router)
    {
      var filters = new List<MqttTopicFilter>();
      foreach (var pattern in router.RouteTable.Keys)
      {
        var patternFilter = new MqttTopicFilterBuilder()
                    .WithTopic(pattern)
                    .Build();
        filters.Add(patternFilter);
      }

      client.SubscribeAsync(filters.ToArray());
    }

    public static async Task PublishEventAsync<T>(this IMqttClient client, string pubSubName, string topic, T data, CancellationToken cancellationToken = default)
    {
      var cloudEvent = new CloudEvent()
      {
        Data = data,
        Source = "daemon",
        Type = "com.dapr.event.sent",
        ExtensionAttributes = new Dictionary<string, object>()
        {
          { "pubsubname", pubSubName },
          { "topic", topic },
        }
      };

      await client.PublishAsync(
                new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(JsonSerializer.Serialize(cloudEvent))
                    .Build(),
                cancellationToken
            );
    }
  }
}