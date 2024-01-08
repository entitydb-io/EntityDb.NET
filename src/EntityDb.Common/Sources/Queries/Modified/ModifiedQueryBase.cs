using EntityDb.Abstractions.Sources.Queries;

namespace EntityDb.Common.Sources.Queries.Modified;

internal abstract record ModifiedQueryBase(IQuery Query, ModifiedQueryOptions ModifiedQueryOptions)
{
    public int? Skip => ModifiedQueryOptions.ReplaceSkip ?? Query.Skip;

    public int? Take => ModifiedQueryOptions.ReplaceTake ?? Query.Take;

    public object? Options => Query.Options;
}
