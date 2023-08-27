using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using EntityDb.Common.Snapshots;
using EntityDb.EntityFramework.DbContexts;
using EntityDb.EntityFramework.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EntityDb.EntityFramework.Snapshots;

internal class EntityFrameworkSnapshotRepositoryFactory<TSnapshot, TDbContext> : DisposableResourceBaseClass,
    IEntityFrameworkSnapshotRepositoryFactory<TSnapshot>
    where TSnapshot : class, IEntityFrameworkSnapshot<TSnapshot>
    where TDbContext : DbContext, IEntityDbContext<TDbContext>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsFactory<EntityFrameworkSnapshotSessionOptions> _optionsFactory;
    private readonly IEntityDbContextFactory<TDbContext> _dbContextFactory;

    public EntityFrameworkSnapshotRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IOptionsFactory<EntityFrameworkSnapshotSessionOptions> optionsFactory,
        IEntityDbContextFactory<TDbContext> dbContextFactory
    )
    {
        _serviceProvider = serviceProvider;
        _optionsFactory = optionsFactory;
        _dbContextFactory = dbContextFactory;
    }

    public ISnapshotRepository<TSnapshot> CreateRepository(IEntityFrameworkSession<TSnapshot> entityFrameworkSession)
    {
        var entityFrameworkSnapshotRepository = new EntityFrameworkSnapshotRepository<TSnapshot>
        (
            entityFrameworkSession
        );

        return TryCatchSnapshotRepository<TSnapshot>.Create(_serviceProvider, entityFrameworkSnapshotRepository);
    }

    public Task<IEntityFrameworkSession<TSnapshot>> CreateSession(EntityFrameworkSnapshotSessionOptions options, CancellationToken cancellationToken)
    {
        var dbContext = _dbContextFactory.Create(options);

        return Task.FromResult(EntityFrameworkSession<TSnapshot, TDbContext>.Create(_serviceProvider, dbContext, options));
    }

    public EntityFrameworkSnapshotSessionOptions GetSessionOptions(string snapshotSessionOptionsName)
    {
        return _optionsFactory.Create(snapshotSessionOptionsName);
    }
}
