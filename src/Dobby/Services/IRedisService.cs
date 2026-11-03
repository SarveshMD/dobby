using StackExchange.Redis;

namespace Dobby.Services;

public interface IRedisService
{
    Task<IDatabase> GetDatabase();

}
