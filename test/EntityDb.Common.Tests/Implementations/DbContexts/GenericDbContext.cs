using EntityDb.Common.Tests.Implementations.Snapshots;
using EntityDb.EntityFramework.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

namespace EntityDb.Common.Tests.Implementations.DbContexts;

internal class GenericDbContext<TSnapshot> : SnapshotReferenceDbContext
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new SnapshotReferenceTypeConfiguration<TSnapshot>());
        modelBuilder.ApplyConfiguration(new SnapshotTypeConfiguration<TSnapshot>());
    }
}

public class SnapshotReferenceTypeConfiguration<TSnapshot> : EntityFramework.Snapshots.SnapshotReferenceTypeConfiguration<TSnapshot>
    where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
{
    public SnapshotReferenceTypeConfiguration() : base($"{typeof(TSnapshot).Name}SnapshotReferences")
    {
    }
}

public class SnapshotTypeConfiguration<TSnapshot> : IEntityTypeConfiguration<TSnapshot>
    where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
{
    public virtual void Configure(EntityTypeBuilder<TSnapshot> snapshotBuilder)
    {
        TSnapshot.Configure(snapshotBuilder);
    }
}