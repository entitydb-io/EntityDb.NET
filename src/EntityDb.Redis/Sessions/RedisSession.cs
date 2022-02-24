using EntityDb.Common.Exceptions;
using EntityDb.Common.Snapshots;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Redis.Sessions;

internal sealed record RedisSession(IConnectionMultiplexer ConnectionMultiplexer, SnapshotSessionOptions SnapshotSessionOptions) : IRedisSession
{
    private CommandFlags GetCommandFlags()
    {
        if (!SnapshotSessionOptions.ReadOnly)
        {
            return CommandFlags.DemandMaster;
        }

        return SnapshotSessionOptions.SecondaryPreferred
            ? CommandFlags.PreferReplica
            : CommandFlags.PreferMaster;
    }

    private void AssertNotReadOnly()
    {
        if (SnapshotSessionOptions.ReadOnly)
        {
            throw new CannotWriteInReadOnlyModeException();
        }
    }

    public async Task<bool> Insert(RedisKey redisKey, RedisValue redisValue)
    {
        AssertNotReadOnly();

        var redisTransaction = ConnectionMultiplexer.GetDatabase().CreateTransaction();

        var insertedTask = redisTransaction.StringSetAsync(redisKey, redisValue);

        await redisTransaction.ExecuteAsync(GetCommandFlags());

        return await insertedTask;
    }

    public async Task<RedisValue> Find(RedisKey redisKey)
    {
        var redisDatabase = ConnectionMultiplexer.GetDatabase();

        return await redisDatabase.StringGetAsync(redisKey, GetCommandFlags());
    }

    public async Task<bool> Delete(IEnumerable<RedisKey> redisKeys)
    {
        AssertNotReadOnly();

        var redisTransaction = ConnectionMultiplexer.GetDatabase().CreateTransaction();

        var deleteSnapshotTasks = redisKeys
            .Select(key => redisTransaction.KeyDeleteAsync(key))
            .ToArray();

        await redisTransaction.ExecuteAsync(GetCommandFlags());

        await Task.WhenAll(deleteSnapshotTasks);

        return deleteSnapshotTasks.All(task => task.Result);
    }

    public ValueTask DisposeAsync()
    {
        ConnectionMultiplexer.Dispose();

        return ValueTask.CompletedTask;
    }
}
