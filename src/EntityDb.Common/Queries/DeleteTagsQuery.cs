using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Abstractions.Queries.SortBuilders;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Queries;

internal sealed record DeleteTagsQuery(Id EntityId, IReadOnlyCollection<ITag> Tags, object? Options = null) : ITagQuery
{
    public TFilter GetFilter<TFilter>(ITagFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.EntityIdIn(EntityId),
            builder.Or
            (
                Tags
                    .Select(deleteTag => builder.And(
                        builder.TagLabelEq(deleteTag.Label),
                        builder.TagValueEq(deleteTag.Value)
                    ))
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
