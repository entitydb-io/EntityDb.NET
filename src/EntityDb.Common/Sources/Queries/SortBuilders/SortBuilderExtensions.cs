using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.SortBuilders;

/// <summary>
///     Extensions for sort builders.
/// </summary>
public static class SortBuilderExtensions
{
    /// <summary>
    ///     Returns a <see cref="ISourceDataSortBuilder{TSort}" /> that orders objects in the reverse order of
    ///     another <see cref="ISourceDataSortBuilder{TSort}" />.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    /// <param name="builder">The message group sort builder.</param>
    /// <returns>
    ///     A <see cref="ISourceDataSortBuilder{TSort}" /> that orders objects in the reverse order of
    ///     <paramref name="builder" />.
    /// </returns>
    public static ISourceDataSortBuilder<TSort> Reverse<TSort>(this ISourceDataSortBuilder<TSort> builder)
    {
        return new ReverseSourceDataSortBuilder<TSort> { SourceDataSortBuilder = builder };
    }

    /// <summary>
    ///     Returns a <see cref="IMessageDataSortBuilder{TSort}" /> that orders objects in the reverse order of another
    ///     <see cref="IMessageDataSortBuilder{TSort}" />.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    /// <param name="builder">The message sort builder.</param>
    /// <returns>
    ///     A <see cref="IMessageDataSortBuilder{TSort}" /> that orders message in the reverse order of
    ///     <paramref name="builder" />.
    /// </returns>
    public static IMessageDataSortBuilder<TSort> Reverse<TSort>(this IMessageDataSortBuilder<TSort> builder)
    {
        return new ReverseMessageDataSortBuilder<TSort> { MessageDataSortBuilder = builder };
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
        return new ReverseLeaseSortBuilder<TSort> { LeaseSortBuilder = builder };
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
        return new ReverseTagSortBuilder<TSort> { TagSortBuilder = builder };
    }
}
