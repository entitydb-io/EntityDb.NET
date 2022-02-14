using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Projections
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TProjection"></typeparam>
    public interface IProjectionStrategy<TProjection>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectionId"></param>
        /// <param name="projectionSnapshot"></param>
        /// <returns></returns>
        Task<Guid[]> GetEntityIds(Guid projectionId, TProjection? projectionSnapshot);
    }
}
