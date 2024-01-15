using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.States;
using StackExchange.Redis;

namespace EntityDb.Redis.States.Sessions;

internal interface IRedisSession : IDisposableResource
{
    Task<bool> Upsert(StatePointer statePointer, RedisValue redisValue);
    Task<RedisValue> Fetch(StatePointer statePointer);
    Task<bool> Delete(StatePointer[] statePointers);
}
