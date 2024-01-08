using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Queries.Standard;

internal sealed record DeleteTagsQuery
    (Id EntityId, IReadOnlyCollection<ITag> Tags, object? Options = null) : ITagQuery
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
