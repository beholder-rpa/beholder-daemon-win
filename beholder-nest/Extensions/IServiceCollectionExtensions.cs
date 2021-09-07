namespace beholder_nest
{
  using beholder_nest.Cache;
  using beholder_nest.Models;
  using beholder_nest.Routing;
  using Microsoft.Extensions.Caching.Memory;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using System.Collections.Generic;
  using System.Reflection;

  public static class IServiceCollectionExtensions
  {
    public static IServiceCollection AddMqttControllers(this IServiceCollection services, ICollection<Assembly> assemblies = null, BeholderServiceInfo serviceInfo = null)
    {
      if (assemblies == null)
      {
        assemblies = new Assembly[] {
          Assembly.GetEntryAssembly()
        };
      }

      if (serviceInfo == null)
      {
        serviceInfo = new BeholderServiceInfo();
      }

      services.AddSingleton<IMemoryCache, MemoryCache>();
      services.AddSingleton<RedisCacheClient>();
      services.AddSingleton<MemoryCacheClient>();
      services.AddSingleton<ICacheClient, AggregateCacheClient>(sp =>
      {
        return new AggregateCacheClient(
          sp.GetRequiredService<MemoryCacheClient>(),
          sp.GetRequiredService<RedisCacheClient>(),
          sp.GetRequiredService<ILogger<AggregateCacheClient>>()
        );
      });

      var routeTable = MqttRouteTableFactory.Create(assemblies, services, new BeholderServiceInfo());
      services.AddSingleton(routeTable);
      services.AddSingleton<MqttApplicationMessageRouter>();

      return services;
    }
  }
}