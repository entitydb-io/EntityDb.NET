using EntityDb.Abstractions.Leases;
using EntityDb.Common.Leases;
using System.Collections.Immutable;

namespace EntityDb.Common.Tests.Implementations.Seeders
{
    public static class LeaseSeeder
    {
        public static ImmutableArray<ILease> Create()
        {
            var lease = new Lease("Foo", "Bar", "Baz");

            return ImmutableArray.Create<ILease>(lease);
        }
    }
}
