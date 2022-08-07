using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.ValueObjects;
using StackExchange.Redis;

namespace EntityDb.Redis.Sessions;

internal interface IRedisSession : IDisposableResource
{
    Task<bool> Insert(Pointer snapshotPointer, RedisValue redisValue);
    Task<RedisValue> Find(Pointer snapshotPointer);
    Task<bool> Delete(Pointer[] snapshotPointers);
}
