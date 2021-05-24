using EntityDb.Abstractions.Queries;

namespace EntityDb.Common.Queries.Filtered
{
    internal abstract record FilteredQueryBase(IQuery Query)
    {
        public int? Skip => Query.Skip;

        public int? Take => Query.Take;
    }
}
