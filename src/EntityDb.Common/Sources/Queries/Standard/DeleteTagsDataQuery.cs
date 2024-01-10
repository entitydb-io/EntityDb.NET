using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.Sources.Queries.FilterBuilders;
using EntityDb.Abstractions.Sources.Queries.SortBuilders;
using EntityDb.Abstractions.States.Attributes;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Sources.Queries.Standard;

internal sealed record DeleteTagsDataQuery
    (Id StateId, IReadOnlyCollection<ITag> Tags, object? Options = null) : ITagDataDataQuery
{
    public TFilter GetFilter<TFilter>(ITagDataFilterBuilder<TFilter> builder)
    {
        return builder.And
        (
            builder.StateIdIn(StateId),
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

    public TSort? GetSort<TSort>(ITagDataSortBuilder<TSort> builder)
    {
        return default;
    }

    public int? Skip => null;

    public int? Take => null;
}
