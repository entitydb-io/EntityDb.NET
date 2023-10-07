using EntityDb.EntityFramework.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EntityDb.EntityFramework.DbContexts;

internal class EntityDbContextFactory<TDbContext> : IEntityDbContextFactory<TDbContext>
    where TDbContext : DbContext, IEntityDbContext<TDbContext>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsFactory<EntityFrameworkSnapshotSessionOptions> _optionsFactory;

    public EntityDbContextFactory(IServiceProvider serviceProvider, IOptionsFactory<EntityFrameworkSnapshotSessionOptions> optionsFactory)
    {
        _serviceProvider = serviceProvider;
        _optionsFactory = optionsFactory;
    }

    public TDbContext Create(string snapshotSessionOptionsName)
    {
        return TDbContext.Construct(_serviceProvider, _optionsFactory.Create(snapshotSessionOptionsName));
    }

    TDbContext IEntityDbContextFactory<TDbContext>.Create(EntityFrameworkSnapshotSessionOptions snapshotSessionOptions)
    {
        return TDbContext.Construct(_serviceProvider, snapshotSessionOptions);
    }
}
