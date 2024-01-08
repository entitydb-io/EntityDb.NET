using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.SortBuilders;

internal abstract record ReverseSortBuilderBase<TSort>(ISortBuilder<TSort> SortBuilder)
{
    public TSort SourceTimeStamp(bool ascending)
    {
        return SortBuilder.SourceTimeStamp(!ascending);
    }

    public TSort SourceId(bool ascending)
    {
        return SortBuilder.SourceId(!ascending);
    }

    public TSort DataType(bool ascending)
    {
        return SortBuilder.DataType(!ascending);
    }

    public TSort Combine(params TSort[] sorts)
    {
        return SortBuilder.Combine(sorts);
    }
}
