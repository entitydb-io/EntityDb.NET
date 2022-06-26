using System;
using System.Linq;
using System.Threading;
using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Projections;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Snapshots;

namespace EntityDb.Common.Tests.Implementations.Projections;

public record OneToOneProjection
(
    Id Id,
    VersionNumber VersionNumber = default,
    VersionNumber EntityVersionNumber = default
) : IProjection<OneToOneProjection>, ISnapshotWithTestMethods<OneToOneProjection>
{
    public const string RedisKeyNamespace = "one-to-one-projection";
    
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

    public VersionNumber GetEntityVersionNumber(Id entityId)
    {
        return EntityVersionNumber;
    }

    public OneToOneProjection Reduce(params IEntityAnnotation<object>[] annotatedCommands)
    {
        return annotatedCommands.Aggregate(this, (previousProjection, nextAnnotatedCommand) => nextAnnotatedCommand switch
        {
            IEntityAnnotation<DoNothing> doNothing => doNothing.Data.Reduce(previousProjection),
            IEntityAnnotation<Count> count => count.Data.Reduce(previousProjection),
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

    public static AsyncLocal<Func<OneToOneProjection, OneToOneProjection?, bool>?> ShouldRecordAsMostRecentLogic { get; } = new();

    public bool ShouldRecordAsMostRecent(OneToOneProjection? previousSnapshot)
    {
        if (ShouldRecordAsMostRecentLogic.Value is not null)
        {
            return ShouldRecordAsMostRecentLogic.Value.Invoke(this, previousSnapshot);
        }
        
        return !Equals(previousSnapshot);
    }
}