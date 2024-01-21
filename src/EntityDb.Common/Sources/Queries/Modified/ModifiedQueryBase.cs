using EntityDb.Abstractions.Sources.Queries;

namespace EntityDb.Common.Sources.Queries.Modified;

internal abstract record ModifiedQueryBase
{
    protected abstract IDataQuery DataQuery { get; }
    public required ModifiedQueryOptions ModifiedQueryOptions { get; init; }

    public int? Skip => ModifiedQueryOptions.ReplaceSkip ?? DataQuery.Skip;

    public int? Take => ModifiedQueryOptions.ReplaceTake ?? DataQuery.Take;

    public object? Options => DataQuery.Options;
}
