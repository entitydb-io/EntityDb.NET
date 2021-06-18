using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Abstractions.Tags;
using EntityDb.Common.Tags;

namespace EntityDb.Common.Queries
{
    /// <summary>
    /// A query for the exact tag.
    /// </summary>
    public sealed record ExactTagQuery(ITag Tag) : ITagQuery
    {
        public TFilter GetFilter<TFilter>(ITagFilterBuilder<TFilter> builder)
        {
            return builder.TagMatches<Tag>((x) =>
                x.Scope == Tag.Scope &&
                x.Label == Tag.Label &&
                x.Value == Tag.Value);
        }

        public TSort? GetSort<TSort>(ITagSortBuilder<TSort> builder)
        {
            return default;
        }

        public int? Skip => null;

        public int? Take => 1;
    }
}
