using EntityDb.Abstractions.Tags;
using EntityDb.Common.Tags;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class TagSeeder
{
    public static ITag Create()
    {
        return new Tag("Foo", "Bar");
    }
}