using EntityDb.Abstractions.Leases;
using EntityDb.Common.Leases;
using System.Collections.Immutable;

namespace EntityDb.Common.Tests.Implementations.Seeders
{
    public static class LeaseSeeder
    {
        public static ILease Create()
        {
            return new Lease("Foo", "Bar", "Baz");
        }
    }
}
