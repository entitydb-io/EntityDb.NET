using EntityDb.Abstractions.ValueObjects;
using System;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Projections;

/// <summary>
///     Represents a type that can map a projection it to a set of entity ids.
/// </summary>
/// <typeparam name="TProjection"></typeparam>
public interface IProjectionStrategy<in TProjection>
{
    /// <summary>
    ///     Map a projection id to a set of entity ids. 
    /// </summary>
    /// <param name="projectionId">The id of the projection.</param>
    /// <param name="projectionSnapshot">A snapshot of the projection, if one exists. (This can be used to avoid running a query, if one were necessary.)</param>
    /// <returns>The set of entity ids to query for creating the projection.</returns>
    Task<Id[]> GetEntityIds(Id projectionId, TProjection projectionSnapshot);

    //TODO: Method to map [entityId, entity] to [projectionId (nullable)]
}
