using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using EntityDb.Common.Snapshots;
using EntityDb.EntityFramework.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EntityDb.EntityFramework.Snapshots;

internal class EntityFrameworkSnapshotRepositoryFactory<TSnapshot, TDbContext> : DisposableResourceBaseClass,
    IEntityFrameworkSnapshotRepositoryFactory<TSnapshot>
    where TSnapshot : class, IEntityFrameworkSnapshot<TSnapshot>
    where TDbContext : DbContext, ISnapshotReferenceDbContext<TDbContext>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsFactory<EntityFrameworkSnapshotSessionOptions> _optionsFactory;

    public EntityFrameworkSnapshotRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IOptionsFactory<EntityFrameworkSnapshotSessionOptions> optionsFactory
    )
    {
        _serviceProvider = serviceProvider;
        _optionsFactory = optionsFactory;
    }

    public ISnapshotRepository<TSnapshot> CreateRepository(IEntityFrameworkSession<TSnapshot> entityFrameworkSession)
    {
        var entityFrameworkSnapshotRepository = new EntityFrameworkSnapshotRepository<TSnapshot>
        (
            entityFrameworkSession
        );

        return TryCatchSnapshotRepository<TSnapshot>.Create(_serviceProvider, entityFrameworkSnapshotRepository);
    }

    public async Task<IEntityFrameworkSession<TSnapshot>> CreateSession(EntityFrameworkSnapshotSessionOptions options, CancellationToken cancellationToken)
    {
        var dbContext = await TDbContext.ConstructAsync(options);

        return EntityFrameworkSession<TSnapshot, TDbContext>.Create(_serviceProvider, dbContext, options);
    }

    public EntityFrameworkSnapshotSessionOptions GetSessionOptions(string snapshotSessionOptionsName)
    {
        return _optionsFactory.Create(snapshotSessionOptionsName);
    }
}
