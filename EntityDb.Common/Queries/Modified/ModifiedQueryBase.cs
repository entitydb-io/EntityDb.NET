using EntityDb.Abstractions.Queries;

namespace EntityDb.Common.Queries.Modified
{
    internal abstract record ModifiedQueryBase(IQuery Query, int? ReplaceSkip, int? ReplaceTake)
    {
        public int? Skip => ReplaceSkip ?? Query.Skip;

        public int? Take => ReplaceTake ?? Query.Take;
    }
}
