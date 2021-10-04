using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;

namespace EntityDb.Common.Queries.Filters
{
    /// <summary>
    ///     Represents a type that supplies additional filtering for a <see cref="ICommandQuery" />.
    /// </summary>
    public interface ICommandFilter
    {
        /// <inheritdoc cref="ICommandQuery.GetFilter{TFilter}(ICommandFilterBuilder{TFilter})" />
        TFilter GetFilter<TFilter>(ICommandFilterBuilder<TFilter> builder);
    }
}
