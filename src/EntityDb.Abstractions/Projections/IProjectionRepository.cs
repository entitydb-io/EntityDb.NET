using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.Snapshots;
using System;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Projections
{
    /// <summary>
    ///     Encapsulates the snapshot repository of a projection (and likely the transaction repository for an entity)
    /// </summary>
    /// <typeparam name="TProjection"></typeparam>
    /// <remarks>
    ///     <c>
    ///         Decided against including TEntity in the interface
    ///         
    ///         <br /><br />
    ///         
    ///         i.e., IProjectionRepository&lt;TEnttiy, TProjection&gt;
    ///         
    ///         instead of:
    ///         
    ///         <br /><br />
    ///         
    ///         IProjectionRepository&gt;TProjection&lt;
    ///     </c>
    /// </remarks>
    public interface IProjectionRepository<TProjection> : IDisposableResource
    {
        /// <summary>
        /// 
        /// </summary>
        public ISnapshotRepository<TProjection> SnapshotRepository { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectionId"></param>
        /// <returns></returns>
        Task<TProjection> GetCurrent(Guid projectionId);
    }
}
