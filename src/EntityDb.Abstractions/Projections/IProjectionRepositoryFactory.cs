using System.Threading.Tasks;

namespace EntityDb.Abstractions.Projections
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TProjection"></typeparam>
    public interface IProjectionRepositoryFactory<TProjection>
    {
        /// <summary>
        ///     Creates a new instance of <see cref="IProjectionRepository{TProjection}"/>.
        /// </summary>
        /// <param name="transactionSessionOptionsName">The agent's use case for the transaction repository.</param>
        /// <param name="snapshotSessionOptionsName">The agent's use case for the snapshot repository.</param>
        /// <returns>A new instance of <see cref="IProjectionRepository{TProjection}"/>.</returns>
        Task<IProjectionRepository<TProjection>> CreateRepository(string transactionSessionOptionsName, string snapshotSessionOptionsName);
    }
}
