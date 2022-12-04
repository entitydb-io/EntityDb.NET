namespace EntityDb.DocumentationGenerator.Extensions;

internal static class StringExtensions
{
    public static string TrimMultiline(this string input)
    {
        return string.Join("\n", input.Split('\n', StringSplitOptions.TrimEntries));
    }
}
