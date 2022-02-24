using EntityDb.Abstractions.Tags;
using EntityDb.Common.Tags;
using System.Collections.Immutable;

namespace EntityDb.Common.Tests.Implementations.Seeders
{
    public static class TagSeeder
    {
        public static ITag Create()
        {
            return new Tag("Foo", "Bar");
        }
    }
}
