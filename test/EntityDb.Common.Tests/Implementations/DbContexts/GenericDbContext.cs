using EntityDb.Common.Tests.Implementations.Snapshots;
using EntityDb.EntityFramework.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

namespace EntityDb.Common.Tests.Implementations.DbContexts;

internal class GenericDbContext<TSnapshot> : SnapshotReferenceDbContext<TSnapshot>
    where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
{
    public GenericDbContext(DbContextOptions<GenericDbContext<TSnapshot>> options) : base(options)
    {
        try
        {
            var databaseCreator = (RelationalDatabaseCreator)Database.GetService<IDatabaseCreator>();

            databaseCreator.CreateTables();
        }
        catch
        {
            // One of the tests has already created the tables.
        }
    }

    protected override SnapshotReferenceTypeConfiguration<TSnapshot> SnapshotReferenceTypeConfiguration { get; } = new();
}

public class SnapshotReferenceTypeConfiguration<TSnapshot> : EntityFramework.Snapshots.SnapshotReferenceTypeConfiguration<TSnapshot>
    where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
{
    public SnapshotReferenceTypeConfiguration() : base($"{typeof(TSnapshot).Name}SnapshotReferences", $"{typeof(TSnapshot).Name}Snapshots")
    {
    }

    protected override void Configure(OwnedNavigationBuilder<SnapshotReference<TSnapshot>, TSnapshot> snapshotBuilder)
    {
        base.Configure(snapshotBuilder);

        TSnapshot.Configure(snapshotBuilder);
    }
}