using EntityDb.Abstractions.ValueObjects;
using EntityDb.EntityFramework.Converters;
using Microsoft.EntityFrameworkCore;

namespace EntityDb.EntityFramework.Snapshots;

/// <summary>
///     A DbContext that adds basic converters for types defined in <see cref="EntityDb.Abstractions.ValueObjects"/>
/// </summary>
public class SnapshotReferenceDbContext : DbContext
{
    /// <inheritdoc cref="DbContext(DbContextOptions)" />
    public SnapshotReferenceDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
    }

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
