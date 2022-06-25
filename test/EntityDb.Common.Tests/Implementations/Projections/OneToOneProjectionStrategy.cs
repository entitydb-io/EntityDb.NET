using System.Threading.Tasks;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Tests.Implementations.Projections;

public class SingleEntityProjectionStrategy : IProjectionStrategy<OneToOneProjection>
{
    public Task<Id[]> GetEntityIds(Id projectionId, OneToOneProjection projectionSnapshot)
    {
        return Task.FromResult(new[] { projectionId });
    }

    public Task<Id> GetProjectionId(Id entityId, object entity)
    {
        return Task.FromResult(entityId);
    }
}