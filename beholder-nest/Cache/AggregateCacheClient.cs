namespace beholder_nest.Cache
{
  using beholder_nest.Extensions;
  using Microsoft.Extensions.Logging;
  using System;
  using System.Threading.Tasks;

  /// <summary>
  /// Represents a cache client that uses the primary cache client for reads
  /// </summary>
  public class AggregateCacheClient : ICacheClient
  {
    private ILogger<AggregateCacheClient> _logger;
    private ICacheClient _primaryCacheClient;
    private ICacheClient _secondaryCacheClient;

    public AggregateCacheClient(ICacheClient primaryCacheClient, ICacheClient secondaryCacheClient, ILogger<AggregateCacheClient> logger)
    {
      _primaryCacheClient = primaryCacheClient ?? throw new ArgumentNullException(nameof(primaryCacheClient));
      _secondaryCacheClient = secondaryCacheClient ?? throw new ArgumentNullException(nameof(secondaryCacheClient));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<byte[]> Base64ByteArrayGet(string key)
    {
      var result = await _primaryCacheClient.Base64ByteArrayGet(key);
      if (result == null)
      {
        _logger.LogInformation($"Primary Cache Client did not contain a Base64ByteArray encoded value for {key}");
        result = await _secondaryCacheClient.Base64ByteArrayGet(key);
        if (result != null)
        {
          await _primaryCacheClient.Base64ByteArraySet(key, result);
        }
      }
      return result;
    }

    public async Task<bool> Base64ByteArraySet(string key, byte[] value, TimeSpan? expiry = null)
    {
      return await _primaryCacheClient.Base64ByteArraySet(key, value, expiry);
    }

    public async Task<T> JsonGet<T>(string key)
    {
      var result = await _primaryCacheClient.JsonGet<T>(key);
      if (result == null)
      {
        _logger.LogInformation($"Primary Cache Client did not contain a Json encoded value for {key}");
        result = await _secondaryCacheClient.JsonGet<T>(key);
        if (result != null)
        {
          await _primaryCacheClient.JsonSet(key, result);
        }
      }
      return result;
    }

    public async Task<bool> JsonSet<T>(string key, T value, TimeSpan? expiry = null)
    {
      return await _primaryCacheClient.JsonSet<T>(key, value, expiry);
    }
  }
}
