namespace EntityDb.Common.Extensions;

internal static class EnumerableExtensions
{
    public static IEnumerable<T> ConcatOrCoalesce<T>(this IEnumerable<T>? first, IEnumerable<T> second)
    {
        return first != default
            ? first.Concat(second)
            : second;
    }

    public static IEnumerable<T> AppendOrStart<T>(this IEnumerable<T>? source, T element)
    {
        return source != default
            ? source.Append(element)
            : new[] { element };
    }
}
