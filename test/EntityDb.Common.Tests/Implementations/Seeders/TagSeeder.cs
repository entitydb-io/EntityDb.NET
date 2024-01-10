using EntityDb.Abstractions.States.Attributes;
using EntityDb.Common.States.Attributes;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class TagSeeder
{
    public static ITag Create()
    {
        return new Tag("Foo", "Bar");
    }
}
