using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EntityDb.Redis.Snapshots.Sessions;

internal sealed record RedisSession
(
    ILogger<RedisSession> Logger,
    IDatabase Database,
    RedisSnapshotSessionOptions Options
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
                "Finished Running Redis Insert on `{DatabaseIndex}.{RedisKey}`\n\nCommitted: {Committed}",
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
        if (!Options.ReadOnly)
        {
            return CommandFlags.DemandMaster;
        }

        return Options.SecondaryPreferred
            ? CommandFlags.PreferReplica
            : CommandFlags.PreferMaster;
    }

    private void AssertNotReadOnly()
    {
        if (Options.ReadOnly)
        {
            throw new CannotWriteInReadOnlyModeException();
        }
    }

    private RedisKey GetSnapshotKey(Pointer snapshotPointer)
    {
        return $"{Options.KeyNamespace}#{snapshotPointer}";
    }

    public static IRedisSession Create
    (
        IServiceProvider serviceProvider,
        IDatabase database,
        RedisSnapshotSessionOptions options
    )
    {
        return ActivatorUtilities.CreateInstance<RedisSession>(serviceProvider, database, options);
    }
}
