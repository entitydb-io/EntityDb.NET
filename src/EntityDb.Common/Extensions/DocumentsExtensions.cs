using EntityDb.Abstractions.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Extensions;

internal static class DocumentsExtensions
{
    public static IAsyncEnumerable<Id> EnumerateIds<TDocument>
    (
        this IAsyncEnumerable<TDocument> documents,
        int? skip,
        int? limit,
        Func<IAsyncEnumerable<TDocument>, IAsyncEnumerable<Id>> mapToIds
    )
    {
        var ids = mapToIds
            .Invoke(documents)
            .Distinct();

        if (skip.HasValue)
        {
            ids = ids.Skip(skip.Value);
        }

        if (limit.HasValue)
        {
            ids = ids.Take(limit.Value);
        }

        return ids;
    }
}
