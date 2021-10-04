using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;

namespace EntityDb.Common.Queries.Filters
{
    /// <summary>
    ///     Represents a type that supplies additional filtering for a <see cref="ILeaseQuery" />.
    /// </summary>
    public interface ILeaseFilter
    {
        /// <inheritdoc cref="ILeaseQuery.GetFilter{TFilter}(ILeaseFilterBuilder{TFilter})" />
        TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder);
    }
}
