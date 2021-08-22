﻿namespace MQTTnet.AspNetCore.Extensions
{
  using beholder_nest.Routing;
  using Microsoft.Extensions.DependencyInjection;
  using MQTTnet.Server;
  using System;

  public static class MqttServerOptionsBuilderExtensions
  {
    public static MqttServerOptionsBuilder WithSubscriptions(this MqttServerOptionsBuilder builder, IServiceProvider applicationServices)
    {
      var router = applicationServices.GetRequiredService<MqttApplicationMessageRouter>();
      builder.WithApplicationMessageInterceptor(router);
      return builder;
    }
  }
}