using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Snapshots;
using EntityDb.Common.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Redis.Sessions;

internal sealed record RedisSession
(
    ILogger<RedisSession> Logger,
    IDatabase Database,
    SnapshotSessionOptions SnapshotSessionOptions
) : DisposableResourceBaseRecord, IRedisSession
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

        Logger
            .LogInformation
            (
                "Started Running Redis Insert on `{DatabaseIndex}.{RedisKey}`",
                Database.Database,
                redisKey.ToString()
            );
        
        var redisTransaction = Database.CreateTransaction();
        
        var insertedTask = redisTransaction.StringSetAsync(redisKey, redisValue);

        await redisTransaction.ExecuteAsync(GetCommandFlags());

        var inserted = await insertedTask;
        
        Logger
            .LogInformation
            (
                "Finished Running Redis Insert on `{DatabaseIndex}.{RedisKey}`\n\nInserted: {Inserted}",
                Database.Database,
                redisKey.ToString(),
                inserted
            );

        return inserted;
    }

    public async Task<RedisValue> Find(RedisKey redisKey)
    {
        Logger
            .LogInformation
            (
                "Started Running Redis Query on `{DatabaseIndex}.{RedisKey}`",
                Database.Database,
                redisKey.ToString()
            );
        
        var redisValue = await Database.StringGetAsync(redisKey, GetCommandFlags());
        
        Logger
            .LogInformation
            (
                "Finished Running Redis Query on `{DatabaseIndex}.{RedisKey}`\n\nHas Value: {HasValue}",
                Database.Database,
                redisKey.ToString(),
                redisValue.HasValue
            );

        return redisValue;
    }

    public async Task<bool> Delete(RedisKey[] redisKeys)
    {
        AssertNotReadOnly();

        Logger
            .LogInformation
            (
                "Started Running Redis Delete on `{DatabaseIndex}` for {NumberOfKeys} Key(s)",
                Database.Database,
                redisKeys.Length
            );
        
        var redisTransaction = Database.CreateTransaction();

        var deleteSnapshotTasks = redisKeys
            .Select(key => redisTransaction.KeyDeleteAsync(key))
            .ToArray();

        await redisTransaction.ExecuteAsync(GetCommandFlags());

        await Task.WhenAll(deleteSnapshotTasks);
        
        var allDeleted = deleteSnapshotTasks.All(task => task.Result);

        Logger
            .LogInformation
            (
                "Finished Running Redis Delete on `{DatabaseIndex}` for {NumberOfKeys} Key(s)\n\nAll Deleted: {AllDeleted}",
                Database.Database,
                redisKeys.Length,
                allDeleted
            );

        return allDeleted;
    }

    public static IRedisSession Create
    (
        IServiceProvider serviceProvider,
        IDatabase database,
        SnapshotSessionOptions snapshotSessionOptions
    )
    {
        return ActivatorUtilities.CreateInstance<RedisSession>(serviceProvider, database, snapshotSessionOptions);
    }
}
