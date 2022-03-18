using System;
using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Reducers;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Projections;
using EntityDb.Common.Snapshots;
using EntityDb.Common.Tests.Implementations.Snapshots;

namespace EntityDb.Common.Tests.Implementations.Projections;

public record OneToOneProjection
(
    Id Id,
    VersionNumber EntityVersionNumber = default
) : IProjection<OneToOneProjection>, ISnapshot<OneToOneProjection>, ISnapshotWithVersionNumber<OneToOneProjection>
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
        var newProjection = this;

        foreach (var annotatedCommand in annotatedCommands)
        {
            var command = annotatedCommand.Data;
            
            if (command is not IReducer<OneToOneProjection> reducer)
            {
                throw new NotImplementedException();
            }
            
            newProjection = reducer.Reduce(newProjection);
        }

        return newProjection;
    }

    public bool ShouldReplace(OneToOneProjection? previousSnapshot)
    {
        return !Equals(previousSnapshot);
    }
}