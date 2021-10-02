using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;

namespace EntityDb.Common.Queries.Filtered
{
    /// <summary>
    /// Represents a type that supplies additional filtering for a <see cref="ITagQuery"/>.
    /// </summary>
    public interface ITagFilter
    {
        /// <inheritdoc cref="ITagQuery.GetFilter{TFilter}(ITagFilterBuilder{TFilter})"/>
        TFilter GetFilter<TFilter>(ITagFilterBuilder<TFilter> builder);
    }
}
