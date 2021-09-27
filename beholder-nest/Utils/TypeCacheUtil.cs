namespace beholder_nest.Utils
{
  using Microsoft.Extensions.Caching.Memory;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;

  /// <summary>
  /// Acts as an helper to find a filter types
  /// </summary>
  public static class TypeCacheUtil
  {

    private static readonly MemoryCache Cache = new(new MemoryCacheOptions() { });

    /// <summary>
    /// Find types filtered by a given predicate
    /// </summary>
    /// <param name="cacheKey">The cache key used to store the results</param>
    /// <param name="predicate">The predicate that filters the types</param>
    /// <param name="assemblies">An array containing the assemblies to scan</param>
    /// <returns>The filtered types</returns>
    public static IEnumerable<Type> FindFilteredTypes(string cacheKey, Func<Type, bool> predicate, params Assembly[] assemblies)
    {
      IEnumerable<Type> types;
      List<Type> result;
      if (Cache.TryGetValue(cacheKey, out object cachedValue))
      {
        return (IEnumerable<Type>)cachedValue;
      }
      types = assemblies.ToList().SelectMany(a =>
      {
        try
        {
          return a.DefinedTypes;
        }
        catch
        {
          return Array.Empty<Type>().AsEnumerable();
        }
      });
      result = new List<Type>(types.Count());
      foreach (Type type in types)
      {
        if (predicate(type))
        {
          result.Add(type);
        }
      }
      return result;
    }

    /// <summary>
    /// Find types filtered by a given predicate
    /// </summary>
    /// <param name="cacheKey">The cache key used to store the results</param>
    /// <param name="predicate">The predicate that filters the types</param>
    /// <returns>The filtered types</returns>
    public static IEnumerable<Type> FindFilteredTypes(string cacheKey, Func<Type, bool> predicate)
    {
      return FindFilteredTypes(cacheKey, predicate, AssemblyLocator.GetAssemblies().ToArray());
    }

  }

}
