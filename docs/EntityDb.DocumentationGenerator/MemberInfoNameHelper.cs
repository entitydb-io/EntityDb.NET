namespace EntityDb.DocumentationGenerator
{
    public static class MemberInfoNameHelper
    {
        public static string ReplacePeriodWithHash(string name)
        {
            return name.Replace(".", "#");
        }
    }
}