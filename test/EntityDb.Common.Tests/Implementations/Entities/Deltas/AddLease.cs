using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.States.Deltas;

namespace EntityDb.Common.Tests.Implementations.Entities.Deltas;

public sealed record AddLease(ILease Lease) : DoNothing, IAddLeasesDelta<TestEntity>
{
    public IEnumerable<ILease> GetLeases(TestEntity state)
    {
        yield return Lease;
    }
}
