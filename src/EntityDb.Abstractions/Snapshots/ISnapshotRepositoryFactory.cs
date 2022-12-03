using EntityDb.Abstractions.Disposables;

namespace EntityDb.Abstractions.Snapshots;

/// <summary>
///     Represents a type used to create instances of <see cref="ISnapshotRepository{TSnapshot}" />
/// </summary>
/// <typeparam name="TSnapshot">The type of snapshot stored by the <see cref="ISnapshotRepository{TSnapshot}" />.</typeparam>
public interface ISnapshotRepositoryFactory<TSnapshot> : IDisposableResource
{
    /// <summary>
    ///     Create a new instance of <see cref="ISnapshotRepository{TSnapshot}" />
    /// </summary>
    /// <param name="snapshotSessionOptionsName">The agent's use case for the repository.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A new instance of <see cref="ISnapshotRepository{TSnapshot}" />.</returns>
    Task<ISnapshotRepository<TSnapshot>> CreateRepository(string snapshotSessionOptionsName,
        CancellationToken cancellationToken = default);
}
