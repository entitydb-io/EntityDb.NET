using EntityDb.Abstractions.Sources.Queries;

namespace EntityDb.Common.Sources.Queries.Modified;

internal abstract record ModifiedQueryBase
{
    protected abstract IQuery Query { get; }
    public required ModifiedQueryOptions ModifiedQueryOptions { get; init; }

    public int? Skip => ModifiedQueryOptions.ReplaceSkip ?? Query.Skip;

    public int? Take => ModifiedQueryOptions.ReplaceTake ?? Query.Take;

    public object? Options => Query.Options;
}
