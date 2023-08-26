using EntityDb.Abstractions.ValueObjects;
using EntityDb.EntityFramework.Converters;
using EntityDb.EntityFramework.Sessions;
using Microsoft.EntityFrameworkCore;

namespace EntityDb.EntityFramework.Snapshots;

public interface ISnapshotReferenceDbContext<TDbContext> where TDbContext : ISnapshotReferenceDbContext<TDbContext>
{
    static abstract Task<TDbContext> ConstructAsync(EntityFrameworkSnapshotSessionOptions entityFrameworkSnapshotSessionOptions);
}

/// <summary>
///     A DbContext that adds basic converters for types defined in <see cref="EntityDb.Abstractions.ValueObjects"/>
/// </summary>
public class SnapshotReferenceDbContext : DbContext
{
    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Id>()
            .HaveConversion<IdConverter>();

        configurationBuilder
            .Properties<VersionNumber>()
            .HaveConversion<VersionNumberConverter>();

        configurationBuilder
            .Properties<TimeStamp>()
            .HaveConversion<TimeStampConverter>();
    }
}
