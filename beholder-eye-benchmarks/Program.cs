namespace beholder_eye_benchmarks
{
  //using BenchmarkDotNet.Configs;
  using BenchmarkDotNet.Running;

  class Program
  {
    static void Main()
    {
      // Uncomment this and supply to run method as an argument if troubleshooting/debugging.
      //var config = new BenchmarkDotNet.Configs.DebugInProcessConfig().WithOptions(ConfigOptions.DisableOptimizationsValidator);
      BenchmarkRunner.Run<DesktopFrameBenchmarks>();
      //BenchmarkRunner.Run<DesktopDuplicatorBenchmarks>();
    }
  }
}