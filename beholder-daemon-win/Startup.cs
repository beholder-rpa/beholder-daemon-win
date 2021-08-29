﻿namespace beholder_daemon_win
{
  using beholder_eye;
  using beholder_nest;
  using beholder_nest.Models;
  using beholder_psionix;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Hosting;
  using System;
  using System.Reflection;
  using System.Security.Cryptography;

  public class Startup
  {
    private static readonly SHA256 _sha256 = SHA256.Create();

    // This method gets called by the runtime. Use this method to add services to the container.
    public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
    {
      var beholderOptions = hostContext.Configuration.GetSection("Beholder").Get<BeholderOptions>();
      services.Configure<BeholderOptions>(hostContext.Configuration.GetSection("Beholder"));

      services.AddHttpClient("beholder", c =>
      {
        c.BaseAddress = new Uri($"https://{beholderOptions.BaseUrl}", UriKind.Absolute);
      });

      services.AddSingleton<BeholderServiceInfo>();
      services.AddHostedService<BeholderDaemonWorker>();

      services.AddMqttControllers(
        new Assembly[] {
          Assembly.GetAssembly(typeof(Startup)),
          Assembly.Load("beholder-eye"),
          Assembly.Load("beholder-psionix"),
        }
      );
      services.AddSingleton<IBeholderMqttClient, BeholderMqttClient>();

      //// Eye
      services.AddSingleton<HashAlgorithm>(_sha256);
      services.AddSingleton<BeholderEye>();
      services.AddSingleton<IObserver<BeholderEyeEvent>, BeholderEyeObserver >();
      services.AddSingleton<BeholderEyeContext>();

      //// Psionix
      services.AddSingleton<BeholderPsionix>();
      services.AddSingleton<IObserver<BeholderPsionixEvent>, BeholderPsionixObserver>();
    }
  }
}