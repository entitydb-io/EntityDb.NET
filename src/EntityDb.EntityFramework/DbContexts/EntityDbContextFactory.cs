using EntityDb.EntityFramework.Sessions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EntityDb.EntityFramework.DbContexts;

internal class EntityDbContextFactory<TDbContext> : IEntityDbContextFactory<TDbContext>
    where TDbContext : DbContext, IEntityDbContext<TDbContext>
{
    private readonly IOptionsFactory<EntityFrameworkSnapshotSessionOptions> _optionsFactory;

    public EntityDbContextFactory(IOptionsFactory<EntityFrameworkSnapshotSessionOptions> optionsFactory)
    {
        _optionsFactory = optionsFactory;
    }

    public TDbContext Create(string snapshotSessionOptionsName)
    {
        return TDbContext.Construct(_optionsFactory.Create(snapshotSessionOptionsName));
    }

    TDbContext IEntityDbContextFactory<TDbContext>.Create(EntityFrameworkSnapshotSessionOptions snapshotSessionOptions)
    {
        return TDbContext.Construct(snapshotSessionOptions);
    }
}
