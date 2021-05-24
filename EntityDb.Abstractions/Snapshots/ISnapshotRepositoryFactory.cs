using System.Threading.Tasks;

namespace EntityDb.Abstractions.Snapshots
{
    /// <summary>
    /// Represents a type used to create instances of <see cref="ISnapshotRepository{TEntity}"/>
    /// </summary>
    /// <typeparam name="TEntity">The type of entity stored by the <see cref="ISnapshotRepository{TEntity}"/>.</typeparam>
    public interface ISnapshotRepositoryFactory<TEntity>
    {
        /// <summary>
        /// Create a new instance of <see cref="ISnapshotRepository{TEntity}"/>
        /// </summary>
        /// <param name="snapshotSessionOptions">The agent's use case for the repository.</param>
        /// <returns>A new instance of <see cref="ISnapshotRepository{TEntity}"/>.</returns>
        Task<ISnapshotRepository<TEntity>> CreateRepository(ISnapshotSessionOptions snapshotSessionOptions);
    }
}
