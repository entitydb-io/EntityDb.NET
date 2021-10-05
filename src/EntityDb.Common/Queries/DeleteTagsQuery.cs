using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Abstractions.Tags;
using EntityDb.Common.Tags;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Queries
{
    internal sealed record DeleteTagsQuery(Guid EntityId, IReadOnlyCollection<ITag> Tags) : ITagQuery
    {
        public TFilter GetFilter<TFilter>(ITagFilterBuilder<TFilter> builder)
        {
            return builder.And
            (
                builder.EntityIdIn(EntityId),
                builder.Or
                (
                    Tags
                        .Select(deleteLease => builder.TagMatches((Tag lease) =>
                            lease.Label == deleteLease.Label &&
                            lease.Value == deleteLease.Value))
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
