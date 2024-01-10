using EntityDb.Abstractions.States.Attributes;
using EntityDb.Abstractions.States.Deltas;

namespace EntityDb.Common.Tests.Implementations.Entities.Deltas;

public record AddLease(ILease Lease) : DoNothing, IAddLeasesDelta<TestEntity>
{
    public IEnumerable<ILease> GetLeases(TestEntity state)
    {
        yield return Lease;
    }
}
