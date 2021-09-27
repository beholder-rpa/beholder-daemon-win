#nullable enable

namespace beholder_nest.Routing
{
  using MQTTnet;
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  public class MqttRouteTable : Dictionary<string, List<MethodInfo>?>
  {
    public MethodInfo[] GetTopicSubscriptions(MqttApplicationMessage message)
    {
      var result = new List<MethodInfo>();
      foreach (var pattern in Keys)
      {
        if (message.IsTopicMatch(pattern))
        {
          var methods = this[pattern];
          if (methods == null)
          {
            throw new InvalidOperationException($"Internal Error - {pattern} contained a null collection of methods.");
          }

          result.AddRange(methods);
        }
      }

      return result.ToArray();
    }
  }
}