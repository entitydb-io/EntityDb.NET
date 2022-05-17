using System;
using System.Linq;
using System.Threading;
using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Reducers;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Projections;
using EntityDb.Common.Snapshots;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Snapshots;

namespace EntityDb.Common.Tests.Implementations.Projections;

public record OneToOneProjection
(
    Id Id,
    VersionNumber EntityVersionNumber = default
) : IProjection<OneToOneProjection>, ISnapshot<OneToOneProjection>, ISnapshotWithVersionNumber<OneToOneProjection>, ISnapshotWithShouldReplaceLogic<OneToOneProjection>
{
    public const string RedisKeyNamespace = "one-to-one-projection";
    
    public static OneToOneProjection Construct(Id projectionId)
    {
        return new OneToOneProjection(projectionId);
    }
    
    public static OneToOneProjection Construct(Id projectionId, VersionNumber versionNumber)
    {
        return new OneToOneProjection(projectionId, versionNumber);
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

    public static AsyncLocal<Func<OneToOneProjection, OneToOneProjection?, bool>?> ShouldReplaceLogic { get; } = new();

    public bool ShouldReplace(OneToOneProjection? previousSnapshot)
    {
        if (ShouldReplaceLogic.Value != null)
        {
            return ShouldReplaceLogic.Value.Invoke(this, previousSnapshot);
        }
        
        return !Equals(previousSnapshot);
    }
}