using System;
using System.Linq;
using System.Threading;
using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Reducers;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Projections;
using EntityDb.Common.Queries;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Tests.Implementations.Snapshots;

namespace EntityDb.Common.Tests.Implementations.Projections;

public record OneToOneProjection
(
    Id Id,
    VersionNumber VersionNumber = default,
    VersionNumber EntityVersionNumber = default
) : IProjection<OneToOneProjection>, ISnapshotWithTestLogic<OneToOneProjection>
{
    public static string RedisKeyNamespace => "one-to-one-projection";

    public static OneToOneProjection Construct(Id projectionId)
    {
        return new OneToOneProjection(projectionId);
    }

    public OneToOneProjection WithVersionNumber(VersionNumber versionNumber)
    {
        return this with { VersionNumber = versionNumber };
    }

    public Id GetId()
    {
        return Id;
    }

    public VersionNumber GetVersionNumber()
    {
        return VersionNumber;
    }

    public OneToOneProjection Reduce(params IEntityAnnotation<object>[] annotatedCommands)
    {
        return annotatedCommands.Aggregate(this, (previousProjection, nextAnnotatedCommand) => nextAnnotatedCommand switch
        {
            IEntityAnnotation<IReducer<OneToOneProjection>> reducer => reducer.Data.Reduce(previousProjection) with
            {
                EntityVersionNumber = reducer.EntityVersionNumber
            },
            _ => throw new NotSupportedException()
        });
    }

    public static AsyncLocal<Func<OneToOneProjection, bool>?> ShouldRecordLogic { get; } = new();

    public bool ShouldRecord()
    {
        if (ShouldRecordLogic.Value is not null)
        {
            return ShouldRecordLogic.Value.Invoke(this);
        }

        return false;
    }

    public static AsyncLocal<Func<OneToOneProjection, OneToOneProjection?, bool>?> ShouldRecordAsLatestLogic { get; } = new();

    public bool ShouldRecordAsLatest(OneToOneProjection? previousSnapshot)
    {
        if (ShouldRecordAsLatestLogic.Value is not null)
        {
            return ShouldRecordAsLatestLogic.Value.Invoke(this, previousSnapshot);
        }

        return !Equals(previousSnapshot);
    }

    public ICommandQuery GetCommandQuery(Pointer projectionPointer)
    {
        return new GetEntityCommandsQuery(projectionPointer, EntityVersionNumber);
    }

    public static Id? GetProjectionIdOrDefault(object entity)
    {
        if (entity is TestEntity testEntity)
        {
            return testEntity.Id;
        }

        return null;
    }
}