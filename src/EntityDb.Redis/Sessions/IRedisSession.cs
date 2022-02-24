using EntityDb.Abstractions.Disposables;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.Redis.Sessions;

internal interface IRedisSession : IDisposableResource
{
    Task<bool> Insert(RedisKey redisKey, RedisValue redisValue);
    Task<RedisValue> Find(RedisKey redisKey);
    Task<bool> Delete(IEnumerable<RedisKey> redisKeys);
}
