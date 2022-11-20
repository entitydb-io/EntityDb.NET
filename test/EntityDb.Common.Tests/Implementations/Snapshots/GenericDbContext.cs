using EntityDb.EntityFramework.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace EntityDb.Common.Tests.Implementations.Snapshots;

internal class SnapshotDbContext<TSnapshot> : DbContext, ISnapshotDbContext<TSnapshot>
    where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
{
    public DbSet<SnapshotReference<TSnapshot>> SnapshotReferences { get; set; } = default!;
    public DbSet<TSnapshot> Snapshots { get; set; }

    public SnapshotDbContext(DbContextOptions<SnapshotDbContext<TSnapshot>> options) : base(options)
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
            .ApplyConfiguration(new SnapshotReferenceTypeConfiguration<TSnapshot>())
            .ApplyConfiguration(new SnapshotTypeConfiguration<TSnapshot>());
    }
}
