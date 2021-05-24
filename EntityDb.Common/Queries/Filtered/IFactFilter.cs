using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;

namespace EntityDb.Common.Queries.Filtered
{
    /// <summary>
    /// Represents a type that supplies additional filtering for a <see cref="IFactQuery"/>.
    /// </summary>
    public interface IFactFilter
    {
        /// <inheritdoc cref="IFactQuery.GetFilter{TFilter}(IFactFilterBuilder{TFilter})"/>
        TFilter GetFilter<TFilter>(IFactFilterBuilder<TFilter> builder);
    }
}
