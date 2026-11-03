using StackExchange.Redis;

namespace Dobby.Services;

public class RedisService : IRedisService
{
    private readonly IConfiguration _config;

    public RedisService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<IDatabase> GetDatabase()
    {
        var muxer = await ConnectionMultiplexer.ConnectAsync(
            _config["Redis:ConnectionString"]
            ?? throw new InvalidOperationException("Redis connection string is not configured")
        );

        var db = muxer.GetDatabase();

        return db;
    }
}
