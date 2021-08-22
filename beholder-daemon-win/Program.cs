namespace beholder_daemon_win
{
  using Microsoft.Extensions.Hosting;

  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseWindowsService(options =>
            {
              options.ServiceName = "Beholder Daemon";
            })
            .ConfigureServices(Startup.ConfigureServices);
  }
}