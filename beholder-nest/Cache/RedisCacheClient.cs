namespace beholder_nest.Cache
{
  using beholder_nest.Models;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using StackExchange.Redis;
  using System;
  using System.Text.Json;
  using System.Threading.Tasks;

  public class RedisCacheClient : ICacheClient
  {
    private readonly ConfigurationOptions configuration;
    private readonly Lazy<IConnectionMultiplexer> _connection;
    private readonly ILogger<RedisCacheClient> _logger;

    public RedisCacheClient(IOptions<BeholderOptions> options, ILogger<RedisCacheClient> logger)
    {
      if (options == null || options.Value == null)
      {
        throw new ArgumentNullException(nameof(options));
      }

      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      var connectionOptions = options.Value;

      configuration = new ConfigurationOptions()
      {
        EndPoints = { { connectionOptions.BaseUrl, connectionOptions.RedisPort }, },
        AllowAdmin = connectionOptions.RedisAllowAdmin,
        ClientName = $"beholder-daemon-{connectionOptions.HostName}",
        ReconnectRetryPolicy = new LinearRetry(connectionOptions.RedisRetryDelay),
        AbortOnConnectFail = false,
      };

      _connection = new Lazy<IConnectionMultiplexer>(() =>
      {
        var redis = ConnectionMultiplexer.Connect(configuration);
        redis.ErrorMessage += Connection_ErrorMessage;
        //redis.InternalError += _Connection_InternalError;
        //redis.ConnectionFailed += _Connection_ConnectionFailed;
        //redis.ConnectionRestored += _Connection_ConnectionRestored;
        return redis;
      });
    }

    public IConnectionMultiplexer Connection { get { return _connection.Value; } }

    //for the default database
    public IDatabase Database => Connection.GetDatabase();

    public async Task<T> JsonGet<T>(string key)
    {
      var redisValue = await Database.StringGetAsync(key, CommandFlags.None);
      if (!redisValue.HasValue)
        return default;
      return JsonSerializer.Deserialize<T>(redisValue);
    }

    public async Task<bool> JsonSet<T>(string key, T value, TimeSpan? expiry = null)
    {
      if (value == null) return false;
      return await Database.StringSetAsync(key, JsonSerializer.Serialize(value), expiry, When.Always, CommandFlags.None);
    }

    public async Task<byte[]> Base64ByteArrayGet(string key)
    {
      try
      {
        var redisValue = await Database.StringGetAsync(key, CommandFlags.None);
        if (!redisValue.HasValue)
          return default;

        return Convert.FromBase64String(redisValue);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Unable to retrieve Base64ByteArray: {ex.Message}", ex);
        return null;
      }
    }

    public async Task<bool> Base64ByteArraySet(string key, byte[] value, TimeSpan? expiry = null)
    {
      try
      {
        if (value == null) return false;
        return await Database.StringSetAsync(key, Convert.ToBase64String(value), expiry, When.Always, CommandFlags.None);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Unable to store Base64ByteArray: {ex.Message}", ex);
        return false;
      }
    }

    private void Connection_ErrorMessage(object sender, RedisErrorEventArgs e)
    {
      _logger.LogError($"An error occurred: {e.Message}", e);
    }
  }
}