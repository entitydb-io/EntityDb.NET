using EntityDb.Common.Tests.Implementations.Deltas;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class DeltaSeeder
{
    public static object Create()
    {
        return new DoNothing();
    }
}