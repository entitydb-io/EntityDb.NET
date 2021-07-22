﻿using EntityDb.Abstractions.Queries.FilterBuilders;
using EntityDb.Common.Queries.Filtered;
using EntityDb.TestImplementations.Commands;
using EntityDb.TestImplementations.Facts;
using EntityDb.TestImplementations.Source;
using EntityDb.TestImplementations.Leases;

namespace EntityDb.TestImplementations.Queries
{
    public record CountFilter : ISourceFilter, ICommandFilter, IFactFilter, ILeaseFilter
    {
        public TFilter GetFilter<TFilter>(ISourceFilterBuilder<TFilter> builder)
        {
            return builder.SourceTypeIn(typeof(Counter));
        }

        public TFilter GetFilter<TFilter>(ICommandFilterBuilder<TFilter> builder)
        {
            return builder.CommandTypeIn(typeof(Count));
        }

        public TFilter GetFilter<TFilter>(IFactFilterBuilder<TFilter> builder)
        {
            return builder.FactTypeIn(typeof(Counted));
        }

        public TFilter GetFilter<TFilter>(ILeaseFilterBuilder<TFilter> builder)
        {
            return builder.LeaseTypeIn(typeof(CountLease));
        }
    }
}
