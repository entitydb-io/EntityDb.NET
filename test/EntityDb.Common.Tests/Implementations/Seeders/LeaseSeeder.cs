using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Common.Sources.Attributes;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class LeaseSeeder
{
    public static ILease Create()
    {
        return new Lease("Foo", "Bar", "Baz");
    }
}
