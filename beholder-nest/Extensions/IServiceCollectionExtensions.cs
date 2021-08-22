namespace beholder_nest
{
  using beholder_nest.Models;
  using beholder_nest.Routing;
  using Microsoft.Extensions.DependencyInjection;
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

      services.AddSingleton(c => MqttRouteTableFactory.Create(assemblies, serviceInfo));
      services.AddSingleton<ITypeActivatorCache>(new TypeActivatorCache());
      services.AddSingleton<MqttApplicationMessageRouter>();

      return services;
    }
  }
}