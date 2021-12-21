using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.Redis.Sessions
{
    internal interface IRedisSession : IDisposable, IAsyncDisposable
    {
        Task<bool> Insert(RedisKey redisKey, RedisValue redisValue);
        Task<RedisValue> Find(RedisKey redisKey);
        Task<bool> Delete(IEnumerable<RedisKey> redisKeys);
    }
}
