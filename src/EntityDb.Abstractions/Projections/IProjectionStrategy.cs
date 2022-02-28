using System;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Projections;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TProjection"></typeparam>
public interface IProjectionStrategy<in TProjection>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="projectionId"></param>
    /// <param name="projectionSnapshot"></param>
    /// <returns></returns>
    Task<Guid[]> GetEntityIds(Guid projectionId, TProjection? projectionSnapshot);
}
