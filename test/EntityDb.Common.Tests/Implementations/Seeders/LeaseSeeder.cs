using EntityDb.Abstractions.States.Attributes;
using EntityDb.Common.States.Attributes;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class LeaseSeeder
{
    public static ILease Create()
    {
        return new Lease("Foo", "Bar", "Baz");
    }
}
