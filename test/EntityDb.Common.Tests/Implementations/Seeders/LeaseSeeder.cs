using EntityDb.Abstractions.Leases;
using EntityDb.Common.Leases;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class LeaseSeeder
{
    public static ILease Create()
    {
        return new Lease("Foo", "Bar", "Baz");
    }
}