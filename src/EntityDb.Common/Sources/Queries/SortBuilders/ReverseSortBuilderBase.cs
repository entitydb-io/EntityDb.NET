using EntityDb.Abstractions.Sources.Queries.SortBuilders;

namespace EntityDb.Common.Sources.Queries.SortBuilders;

internal abstract record ReverseSortBuilderBase<TSort>
{
    protected abstract IDataSortBuilder<TSort> DataSortBuilder { get; }

    public TSort SourceTimeStamp(bool ascending)
    {
        return DataSortBuilder.SourceTimeStamp(!ascending);
    }

    public TSort SourceId(bool ascending)
    {
        return DataSortBuilder.SourceId(!ascending);
    }

    public TSort DataType(bool ascending)
    {
        return DataSortBuilder.DataType(!ascending);
    }

    public TSort Combine(params TSort[] sorts)
    {
        return DataSortBuilder.Combine(sorts);
    }
}
