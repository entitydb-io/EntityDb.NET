using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Common.Sources.Attributes;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class TagSeeder
{
    public static ITag Create()
    {
        return new Tag("Foo", "Bar");
    }
}