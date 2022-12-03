using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Common.Extensions;

namespace EntityDb.Common.Queries.Modified;

/// <summary>
///     Options for modified queries, which can be created via <see cref="QueryExtensions" />.
/// </summary>
public record ModifiedQueryOptions
{
    /// <summary>
    ///     If <c>true</c>, then the new query will return the value of <see cref="IFilterBuilder{TFilter}.Not(TFilter)" />
    ///     applied to the filter of the original query. Otherwise, the new query will return the same filter as the original
    ///     query.
    /// </summary>
    public bool InvertFilter { get; init; }

    /// <summary>
    ///     If <c>true</c>, then the new query will pass the opposite values of <c>ascending</c> to the sort builder of the
    ///     original query. Otherwise, the new query will return the same sort as the original query.
    /// </summary>
    public bool ReverseSort { get; init; }

    /// <summary>
    ///     If not <c>null</c>, then the new query will return this value for <see cref="IQuery.Skip" />. Otherwise, the new
    ///     query will return the same <see cref="IQuery.Skip" /> as the original query.
    /// </summary>
    public int? ReplaceSkip { get; init; }

    /// <summary>
    ///     If not <c>null</c>, then the new query will return this value for <see cref="IQuery.Take" />. Otherwise, the new
    ///     query will return the same <see cref="IQuery.Skip" /> as the original query.
    /// </summary>
    public int? ReplaceTake { get; init; }
}
