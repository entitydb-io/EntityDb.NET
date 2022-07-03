using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Snapshots;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Redis.Sessions;

internal sealed record RedisSession
(
    ILogger<RedisSession> Logger,
    IDatabase Database,
    string KeyNamespace,
    SnapshotSessionOptions SnapshotSessionOptions
) : DisposableResourceBaseRecord, IRedisSession
{
    public async Task<bool> Insert(Pointer snapshotPointer, RedisValue redisValue)
    {
        AssertNotReadOnly();

        var redisKey = GetSnapshotKey(snapshotPointer);

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

    public async Task<RedisValue> Find(Pointer snapshotPointer)
    {
        var redisKey = GetSnapshotKey(snapshotPointer);

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

    public async Task<bool> Delete(Pointer[] snapshotPointers)
    {
        AssertNotReadOnly();

        Logger
            .LogInformation
            (
                "Started Running Redis Delete on `{DatabaseIndex}` for {NumberOfKeys} Key(s)",
                Database.Database,
                snapshotPointers.Length
            );

        var redisTransaction = Database.CreateTransaction();

        var deleteSnapshotTasks = snapshotPointers
            .Select(snapshotPointer => redisTransaction.KeyDeleteAsync(GetSnapshotKey(snapshotPointer)))
            .ToArray();

        await redisTransaction.ExecuteAsync(GetCommandFlags());

        await Task.WhenAll(deleteSnapshotTasks);

        var allDeleted = deleteSnapshotTasks.All(task => task.Result);

        Logger
            .LogInformation
            (
                "Finished Running Redis Delete on `{DatabaseIndex}` for {NumberOfKeys} Key(s)\n\nAll Deleted: {AllDeleted}",
                Database.Database,
                snapshotPointers.Length,
                allDeleted
            );

        return allDeleted;
    }

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

    private RedisKey GetSnapshotKey(Pointer snapshotPointer)
    {
        return $"{KeyNamespace}#{snapshotPointer.Id.Value}@{snapshotPointer.VersionNumber.Value}";
    }

    public static IRedisSession Create
    (
        IServiceProvider serviceProvider,
        IDatabase database,
        string keyNamespace,
        SnapshotSessionOptions snapshotSessionOptions
    )
    {
        return ActivatorUtilities.CreateInstance<RedisSession>(serviceProvider, database, keyNamespace,
            snapshotSessionOptions);
    }
}
