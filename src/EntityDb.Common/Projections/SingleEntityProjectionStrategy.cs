using System;
using System.Threading.Tasks;

namespace EntityDb.Common.Projections
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TProjection"></typeparam>
    internal class SingleEntityProjectionStrategy<TProjection> : IProjectionStrategy<TProjection>
    {
        public Task<Guid[]> GetEntityIds(Guid projectionId, TProjection? projectionSnapshot)
        {
            return Task.FromResult(new[] { projectionId });
        }
    }
}
