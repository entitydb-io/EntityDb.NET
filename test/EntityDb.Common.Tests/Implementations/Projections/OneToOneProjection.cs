using System.ComponentModel.DataAnnotations.Schema;
using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Reducers;
using EntityDb.Abstractions.ValueObjects;
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
    public VersionNumber VersionNumber { get; init; }

    [Column("Id")]
    public Guid IdValue
    {
        get => Id.Value;
        init => Id = new Id(value);
    }

    [Column("VersionNumber")]
    public ulong VersionNumberValue
    {
        get => VersionNumber.Value;
        init => VersionNumber = new VersionNumber(value);
    }

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
            .Ignore(oneToOneProjection => oneToOneProjection.Id)
            .Ignore(oneToOneProjection => oneToOneProjection.VersionNumber);

        oneToOneProjectionBuilder
            .HasKey(oneToOneProjection => new
            {
                oneToOneProjection.IdValue,
                oneToOneProjection.VersionNumberValue,
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

    public OneToOneProjection Reduce(IEntityAnnotation<object> annotatedCommand)
    {
        return annotatedCommand switch
        {
            IEntityAnnotation<IReducer<OneToOneProjection>> reducer => reducer.Data.Reduce(this) with
            {
                VersionNumber = reducer.EntityVersionNumber
            },
            _ => throw new NotSupportedException()
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

    public ICommandQuery GetCommandQuery(Pointer projectionPointer)
    {
        return new GetEntityCommandsQuery(projectionPointer, VersionNumber);
    }

    public static Id? GetProjectionIdOrDefault(object entity)
    {
        if (entity is TestEntity testEntity) return testEntity.Id;

        return null;
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