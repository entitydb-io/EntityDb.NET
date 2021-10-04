using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;

namespace EntityDb.Common.Queries.Filters
{
    /// <summary>
    ///     Represents a type that supplies additional filtering for a <see cref="ISourceQuery" />.
    /// </summary>
    public interface ISourceFilter
    {
        /// <inheritdoc cref="ISourceQuery.GetFilter{TFilter}(ISourceFilterBuilder{TFilter})" />
        TFilter GetFilter<TFilter>(ISourceFilterBuilder<TFilter> builder);
    }
}
