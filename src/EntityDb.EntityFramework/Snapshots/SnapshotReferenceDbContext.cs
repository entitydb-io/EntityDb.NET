using EntityDb.Abstractions.ValueObjects;
using EntityDb.EntityFramework.Converters;
using Microsoft.EntityFrameworkCore;

namespace EntityDb.EntityFramework.Snapshots;

/// <summary>
///     A minimal DbContext for snapshot references.
/// </summary>
/// <typeparam name="TSnapshot">The type of the snapshot</typeparam>
public abstract class SnapshotReferenceDbContext<TSnapshot> : DbContext
    where TSnapshot : class
{
    /// <summary>
    ///     A database set for resolving snapshots from snapshot pointers.
    /// </summary>
    public required DbSet<SnapshotReference<TSnapshot>> SnapshotReferences { get; set; }

    /// <inheritdoc cref="DbContext(DbContextOptions)" />
    protected SnapshotReferenceDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
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

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder
            .ApplyConfiguration(new SnapshotReferenceTypeConfiguration<TSnapshot>());
    }
}
