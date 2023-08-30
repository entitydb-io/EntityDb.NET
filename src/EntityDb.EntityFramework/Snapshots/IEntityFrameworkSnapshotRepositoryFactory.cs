using EntityDb.Abstractions.Snapshots;
using EntityDb.EntityFramework.Sessions;

namespace EntityDb.EntityFramework.Snapshots;

internal interface IEntityFrameworkSnapshotRepositoryFactory<TSnapshot> : ISnapshotRepositoryFactory<TSnapshot>
{
    async Task<ISnapshotRepository<TSnapshot>> ISnapshotRepositoryFactory<TSnapshot>.CreateRepository(
        string snapshotSessionOptionsName, CancellationToken cancellationToken)
    {
        var options = GetSessionOptions(snapshotSessionOptionsName);

        var entityFrameworkSession = await CreateSession(options, cancellationToken);

        return CreateRepository(entityFrameworkSession);
    }

    EntityFrameworkSnapshotSessionOptions GetSessionOptions(string snapshotSessionOptionsName);

    Task<IEntityFrameworkSession<TSnapshot>> CreateSession(EntityFrameworkSnapshotSessionOptions options,
        CancellationToken cancellationToken);

    ISnapshotRepository<TSnapshot> CreateRepository(IEntityFrameworkSession<TSnapshot> entityFrameworkSession);
}
