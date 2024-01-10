using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EntityDb.Redis.States.Sessions;

internal sealed record RedisSession
(
    ILogger<RedisSession> Logger,
    IDatabase Database,
    RedisStateSessionOptions Options
) : DisposableResourceBaseRecord, IRedisSession
{
    public async Task<bool> Upsert(Pointer statePointer, RedisValue redisValue)
    {
        AssertNotReadOnly();

        var redisKey = GetRedisKey(statePointer);

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

        var upserted = await insertedTask;

        Logger
            .LogInformation
            (
                "Finished Running Redis Insert on `{DatabaseIndex}.{RedisKey}`\n\nUpserted: {Upserted}",
                Database.Database,
                redisKey.ToString(),
                upserted
            );

        return upserted;
    }

    public async Task<RedisValue> Fetch(Pointer statePointer)
    {
        var redisKey = GetRedisKey(statePointer);

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

    public async Task<bool> Delete(Pointer[] statePointers)
    {
        AssertNotReadOnly();

        Logger
            .LogInformation
            (
                "Started Running Redis Delete on `{DatabaseIndex}` for {NumberOfKeys} Key(s)",
                Database.Database,
                statePointers.Length
            );

        var redisTransaction = Database.CreateTransaction();

        var deleteStateTasks = statePointers
            .Select(statePointer => redisTransaction.KeyDeleteAsync(GetRedisKey(statePointer)))
            .ToArray();

        await redisTransaction.ExecuteAsync(GetCommandFlags());

        await Task.WhenAll(deleteStateTasks);

        var allDeleted = deleteStateTasks.All(task => task.Result);

        Logger
            .LogInformation
            (
                "Finished Running Redis Delete on `{DatabaseIndex}` for {NumberOfKeys} Key(s)\n\nAll Deleted: {AllDeleted}",
                Database.Database,
                statePointers.Length,
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
            throw new ReadOnlyWriteException();
        }
    }

    private RedisKey GetRedisKey(Pointer statePointer)
    {
        return $"{Options.KeyNamespace}#{statePointer}";
    }

    public static IRedisSession Create
    (
        IServiceProvider serviceProvider,
        IDatabase database,
        RedisStateSessionOptions options
    )
    {
        return ActivatorUtilities.CreateInstance<RedisSession>(serviceProvider, database, options);
    }
}
