namespace beholder_occipital
{
  using beholder_occipital.ObjectDetection;
  using Microsoft.Extensions.DependencyInjection;
  using System;

  public static class IServiceCollectionExtensions
  {
    public static IServiceCollection AddOccipital(this IServiceCollection services)
    {
      services.AddTransient<IMatchMaskFactory, MatchMaskFactory>();
      services.AddTransient<IMatchProcessor, SiftFlannMatchProcessor>();

      services.AddSingleton<BeholderOccipital>();
      services.AddSingleton<IObserver<BeholderOccipitalEvent>, BeholderOccipitalObserver>();

      return services;
    }
  }
}