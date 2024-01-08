using EntityDb.Abstractions.Entities.Deltas;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Common.Tests.Implementations.Entities;

namespace EntityDb.Common.Tests.Implementations.Deltas;

public record DeleteLease(ILease Lease) : DoNothing, IDeleteLeasesDelta<TestEntity>
{
    public IEnumerable<ILease> GetLeases(TestEntity entity)
    {
        yield return Lease;
    }
}