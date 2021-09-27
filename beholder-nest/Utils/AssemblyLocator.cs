namespace beholder_nest.Utils
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;

  /// <summary>
  /// Acts as a helper class for locating <see cref="Assembly"/> instances
  /// </summary>
  public static class AssemblyLocator
  {

    private static readonly object Lock = new();

    private static readonly List<Assembly> LoadedAssemblies = new();

    static AssemblyLocator()
    {
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        LoadedAssemblies.Add(assembly);
      }
      AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
    }

    /// <summary>
    /// Get all loaded assemlies
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of all loaded assemblies</returns>
    public static IEnumerable<Assembly> GetAssemblies()
    {
      return LoadedAssemblies.AsEnumerable();
    }

    private static void OnAssemblyLoad(object sender, AssemblyLoadEventArgs e)
    {
      lock (Lock)
      {
        LoadedAssemblies.Add(e.LoadedAssembly);
      }
    }

  }
}
