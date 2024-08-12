namespace EntityDb.Abstractions.Extensions;

internal static class EnumerableExtensions
{
    public static IEnumerable<T> ConcatOrCoalesce<T>(this IEnumerable<T>? first, IEnumerable<T> second)
    {
        return first == default ? second : first.Concat(second);
    }

    public static IEnumerable<T> AppendOrStart<T>(this IEnumerable<T>? source, T element)
    {
        return source == default ? new[] { element } : source.Append(element);
    }
}
