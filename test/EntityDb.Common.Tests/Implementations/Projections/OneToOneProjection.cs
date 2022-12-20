using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Reducers;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Transactions;
using EntityDb.Common.Projections;
using EntityDb.Common.Queries;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Tests.Implementations.Snapshots;
using EntityDb.EntityFramework.Snapshots;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pointer = EntityDb.Abstractions.ValueObjects.Pointer;

namespace EntityDb.Common.Tests.Implementations.Projections;

public record OneToOneProjection : IProjection<OneToOneProjection>, ISnapshotWithTestLogic<OneToOneProjection>
{
    public required Id Id { get; init; }
    public TimeStamp LastEventAt { get; init; }
    public VersionNumber VersionNumber { get; init; }

    public static OneToOneProjection Construct(Id projectionId)
    {
        return new OneToOneProjection
        { 
            Id = projectionId,
        };
    }

    public static void Configure(OwnedNavigationBuilder<SnapshotReference<OneToOneProjection>, OneToOneProjection> oneToOneProjectionBuilder)
    {
        oneToOneProjectionBuilder
            .HasKey(oneToOneProjection => new
            {
                oneToOneProjection.Id,
                oneToOneProjection.VersionNumber,
            });
    }

    public Id GetId()
    {
        return Id;
    }

    public VersionNumber GetVersionNumber()
    {
        return VersionNumber;
    }

    public OneToOneProjection Reduce(ITransaction transaction, ITransactionCommand transactionCommand)
    {
        if (transactionCommand.Command is not IReducer<OneToOneProjection> reducer)
        {
            throw new NotSupportedException();
        }

        return reducer.Reduce(this) with
        {
            LastEventAt = transaction.TimeStamp,
            VersionNumber = transactionCommand.EntityVersionNumber
        };
    }

    public bool ShouldRecord()
    {
        return ShouldRecordLogic.Value is not null && ShouldRecordLogic.Value.Invoke(this);
    }

    public bool ShouldRecordAsLatest(OneToOneProjection? previousSnapshot)
    {
        return ShouldRecordAsLatestLogic.Value is not null && ShouldRecordAsLatestLogic.Value.Invoke(this, previousSnapshot);
    }

    public Task<ICommandQuery> GetReducersQuery(Pointer projectionPointer, ITransactionRepository transactionRepository, CancellationToken cancellationToken)
    {
        return Task.FromResult<ICommandQuery>(new GetEntityCommandsQuery(projectionPointer, VersionNumber));
    }

    public static Id? GetProjectionIdOrDefault(ITransaction transaction, ITransactionCommand transactionCommand)
    {
        if (transactionCommand is not ITransactionCommandWithSnapshot transactionCommandWithSnapshot || transactionCommandWithSnapshot.Snapshot is not TestEntity testEntity)
        {
            return null;
        }

        return testEntity.Id;
    }

    public static string RedisKeyNamespace => "one-to-one-projection";

    public OneToOneProjection WithVersionNumber(VersionNumber versionNumber)
    {
        return this with { VersionNumber = versionNumber };
    }

    public static AsyncLocal<Func<OneToOneProjection, bool>?> ShouldRecordLogic { get; } = new();

    public static AsyncLocal<Func<OneToOneProjection, OneToOneProjection?, bool>?> ShouldRecordAsLatestLogic { get; } =
        new();
}