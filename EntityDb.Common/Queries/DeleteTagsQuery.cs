using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Abstractions.Tags;
using EntityDb.Common.Tags;
using System;
using System.Linq;

namespace EntityDb.Common.Queries
{
    internal sealed record DeleteTagsQuery(Guid EntityId, ITag[] Tags) : ITagQuery
    {
        public TFilter GetFilter<TFilter>(ITagFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                builder.EntityIdIn(EntityId),
                builder.Or
                (
                    Tags
                        .Select(deleteTag => builder.TagMatches((Tag tag) =>
                            tag.Scope == deleteTag.Scope &&
                            tag.Label == deleteTag.Label &&
                            tag.Value == deleteTag.Value))
                        .ToArray()
                )
            );
        }

        public TSort? GetSort<TSort>(ITagSortBuilder<TSort> builder)
        {
            return default;
        }

        public int? Skip => null;

        public int? Take => null;
    }
}
