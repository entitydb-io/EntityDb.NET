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
    ///     Returns a <see cref="ILeaseDataSortBuilder{TSort}" /> that orders leases in the reverse order of another
    ///     <see cref="ILeaseDataSortBuilder{TSort}" />.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    /// <param name="builder">The lease sort builder.</param>
    /// <returns>
    ///     A <see cref="ILeaseDataSortBuilder{TSort}" /> that orders leases in the reverse order of
    ///     <paramref name="builder" />.
    /// </returns>
    public static ILeaseDataSortBuilder<TSort> Reverse<TSort>(this ILeaseDataSortBuilder<TSort> builder)
    {
        return new ReverseLeaseDataSortBuilder<TSort> { LeaseDataSortBuilder = builder };
    }

    /// <summary>
    ///     Returns a <see cref="ITagDataSortBuilder{TSort}" /> that orders tags in the reverse order of another
    ///     <see cref="ITagDataSortBuilder{TSort}" />.
    /// </summary>
    /// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
    /// <param name="builder">The tag sort builder.</param>
    /// <returns>
    ///     A <see cref="ITagDataSortBuilder{TSort}" /> that orders tags in the reverse order of
    ///     <paramref name="builder" />.
    /// </returns>
    public static ITagDataSortBuilder<TSort> Reverse<TSort>(this ITagDataSortBuilder<TSort> builder)
    {
        return new ReverseTagDataSortBuilder<TSort> { TagDataSortBuilder = builder };
    }
}
