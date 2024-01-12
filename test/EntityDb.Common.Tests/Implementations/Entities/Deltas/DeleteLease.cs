using EntityDb.Abstractions.States.Attributes;
using EntityDb.Abstractions.States.Deltas;

namespace EntityDb.Common.Tests.Implementations.Entities.Deltas;

public sealed record DeleteLease(ILease Lease) : DoNothing, IDeleteLeasesDelta<TestEntity>
{
    public IEnumerable<ILease> GetLeases(TestEntity state)
    {
        yield return Lease;
    }
}
