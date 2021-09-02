namespace beholder_nest.Cache
{
  using Microsoft.Extensions.Caching.Memory;
  using Microsoft.Extensions.Logging;
  using System;
  using System.Text.Json;
  using System.Threading.Tasks;

  public class MemoryCacheClient : ICacheClient
  {
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<RedisCacheClient> _logger;

    public MemoryCacheClient(IMemoryCache memoryCache, ILogger<RedisCacheClient> logger)
    {
      _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<T> JsonGet<T>(string key)
    {
      var value = _memoryCache.Get<string>($"json_{key}");
      if (value == null) return Task.FromResult<T>(default);
      return Task.FromResult(JsonSerializer.Deserialize<T>(value));
    }

    public Task<bool> JsonSet<T>(string key, T value, TimeSpan? expiry = null)
    {
      if (value == null) return Task.FromResult(false);
      if (expiry == null)
        _memoryCache.Set($"json_{key}", JsonSerializer.Serialize(value));
      else
        _memoryCache.Set($"json_{key}", JsonSerializer.Serialize(value), expiry.Value);
      return Task.FromResult(true);
    }

    public Task<byte[]> Base64ByteArrayGet(string key)
    {
      return Task.FromResult(_memoryCache.Get<byte[]>($"byteArray_{key}"));
    }

    public Task<bool> Base64ByteArraySet(string key, byte[] value, TimeSpan? expiry = null)
    {
      if (expiry == null)
        _memoryCache.Set($"byteArray_{key}", value);
      else
        _memoryCache.Set($"byteArray_{key}", expiry.Value);
      return Task.FromResult(true);
    }
  }
}
