using EntityDb.Common.Tests.Implementations.Commands;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class CommandSeeder
{
    public static object Create()
    {
        return new DoNothing();
    }
}