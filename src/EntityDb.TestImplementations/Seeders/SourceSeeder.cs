using EntityDb.TestImplementations.Source;

namespace EntityDb.TestImplementations.Seeders
{
    public static class SourceSeeder
    {
        public static object Create()
        {
            return new NoSource();
        }
    }
}
