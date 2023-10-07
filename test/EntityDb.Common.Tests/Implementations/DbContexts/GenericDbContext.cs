using EntityDb.Common.Tests.Implementations.Snapshots;
using EntityDb.EntityFramework.DbContexts;
using EntityDb.EntityFramework.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.Tests.Implementations.DbContexts;

internal class GenericDbContext<TSnapshot> : EntityDbContextBase, IEntityDbContext<GenericDbContext<TSnapshot>>
    where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly EntityFrameworkSnapshotSessionOptions _options;

    public GenericDbContext(ILoggerFactory loggerFactory, EntityFrameworkSnapshotSessionOptions options)
    {
        _loggerFactory = loggerFactory;
        _options = options;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql($"{_options.ConnectionString};Include Error Detail=true")
            .UseLoggerFactory(_loggerFactory)
            .EnableSensitiveDataLogging();
    }

    public static GenericDbContext<TSnapshot> Construct(IServiceProvider serviceProvider, EntityFrameworkSnapshotSessionOptions entityFrameworkSnapshotSessionOptions)
    {
        return ActivatorUtilities.CreateInstance<GenericDbContext<TSnapshot>>(serviceProvider, entityFrameworkSnapshotSessionOptions);
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