using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.ValueObjects;
using StackExchange.Redis;

namespace EntityDb.Redis.States.Sessions;

internal interface IRedisSession : IDisposableResource
{
    Task<bool> Upsert(Pointer statePointer, RedisValue redisValue);
    Task<RedisValue> Fetch(Pointer statePointer);
    Task<bool> Delete(Pointer[] statePointers);
}
