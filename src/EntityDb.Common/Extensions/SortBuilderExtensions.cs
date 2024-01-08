using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Common.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Extensions;

/// <summary>
///     Extensions for sort builders.
/// </summary>
public static class SortBuilderExtensions
{
    /// <summary>
    ///     Returns a <see cref="IMessageGroupSortBuilder{TSort}" /> that orders objects in the reverse order of
    ///     another <see cref="IMessageGroupSortBuilder{TSort}" />.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    /// <param name="builder">The message group sort builder.</param>
    /// <returns>
    ///     A <see cref="IMessageGroupSortBuilder{TSort}" /> that orders objects in the reverse order of
    ///     <paramref name="builder" />.
    /// </returns>
    public static IMessageGroupSortBuilder<TSort> Reverse<TSort>(this IMessageGroupSortBuilder<TSort> builder)
    {
        return new ReverseMessageGroupSortBuilder<TSort>(builder);
    }

    /// <summary>
    ///     Returns a <see cref="IMessageSortBuilder{TSort}" /> that orders objects in the reverse order of another
    ///     <see cref="IMessageSortBuilder{TSort}" />.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    /// <param name="builder">The message sort builder.</param>
    /// <returns>
    ///     A <see cref="IMessageSortBuilder{TSort}" /> that orders message in the reverse order of
    ///     <paramref name="builder" />.
    /// </returns>
    public static IMessageSortBuilder<TSort> Reverse<TSort>(this IMessageSortBuilder<TSort> builder)
    {
        return new ReverseMessageSortBuilder<TSort>(builder);
    }

    /// <summary>
    ///     Returns a <see cref="ILeaseSortBuilder{TSort}" /> that orders leases in the reverse order of another
    ///     <see cref="ILeaseSortBuilder{TSort}" />.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    /// <param name="builder">The lease sort builder.</param>
    /// <returns>
    ///     A <see cref="ILeaseSortBuilder{TSort}" /> that orders leases in the reverse order of
    ///     <paramref name="builder" />.
    /// </returns>
    public static ILeaseSortBuilder<TSort> Reverse<TSort>(this ILeaseSortBuilder<TSort> builder)
    {
        return new ReverseLeaseSortBuilder<TSort>(builder);
    }

    /// <summary>
    ///     Returns a <see cref="ITagSortBuilder{TSort}" /> that orders tags in the reverse order of another
    ///     <see cref="ITagSortBuilder{TSort}" />.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    /// <param name="builder">The tag sort builder.</param>
    /// <returns>
    ///     A <see cref="ITagSortBuilder{TSort}" /> that orders tags in the reverse order of
    ///     <paramref name="builder" />.
    /// </returns>
    public static ITagSortBuilder<TSort> Reverse<TSort>(this ITagSortBuilder<TSort> builder)
    {
        return new ReverseTagSortBuilder<TSort>(builder);
    }
}
