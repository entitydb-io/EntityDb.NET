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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .ApplyConfiguration(new SnapshotReferenceTypeConfiguration<TSnapshot>());
    }
}


public class SnapshotReferenceTypeConfiguration<TSnapshot> : EntityFramework.Snapshots.SnapshotReferenceTypeConfiguration<TSnapshot>
    where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
{
    public override void Configure(EntityTypeBuilder<SnapshotReference<TSnapshot>> snapshotReferenceBuilder)
    {
        base.Configure(snapshotReferenceBuilder);

        snapshotReferenceBuilder.ToTable($"{typeof(TSnapshot).Name}SnapshotReferences");
    }

    protected override void Configure(OwnedNavigationBuilder<SnapshotReference<TSnapshot>, TSnapshot> snapshotBuilder)
    {
        base.Configure(snapshotBuilder);

        snapshotBuilder.ToTable($"{typeof(TSnapshot).Name}Snapshots");

        TSnapshot.Configure(snapshotBuilder);
    }
}