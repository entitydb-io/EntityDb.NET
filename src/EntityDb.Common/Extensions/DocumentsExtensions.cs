using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Annotations;
using EntityDb.Common.Documents;
using EntityDb.Common.Envelopes;
using System.Runtime.CompilerServices;

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

    public static async IAsyncEnumerable<IEntityAnnotation<TData>> EnumerateEntityAnnotation<TDocument, TSerializedData, TData>
    (
        this IAsyncEnumerable<TDocument> documents,
        IEnvelopeService<TSerializedData> envelopeService,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
        where TDocument : IEntityDocument<TSerializedData>
        where TData : notnull
    {
        await using var enumerator = documents.GetAsyncEnumerator(cancellationToken);

        while (await enumerator.MoveNextAsync(cancellationToken))
        {
            var document = enumerator.Current;

            yield return EntityAnnotation<TData>.CreateFromBoxedData
            (
                document.TransactionId,
                document.TransactionTimeStamp,
                document.EntityId,
                document.EntityVersionNumber,
                envelopeService.Deserialize<TData>(document.Data)
            );
        }
    }

    public static async IAsyncEnumerable<IEntitiesAnnotation<TData>> EnumerateEntitiesAnnotation<TDocument, TSerializedData, TData>
    (
        this IAsyncEnumerable<TDocument> documents,
        IEnvelopeService<TSerializedData> envelopeService,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
        where TDocument : IEntitiesDocument<TSerializedData>
        where TData : notnull
    {
        await using var enumerator = documents.GetAsyncEnumerator(cancellationToken);

        while (await enumerator.MoveNextAsync(cancellationToken))
        {
            var document = enumerator.Current;

            yield return EntitiesAnnotation<TData>.CreateFromBoxedData
            (
                document.TransactionId,
                document.TransactionTimeStamp,
                document.EntityIds,
                envelopeService.Deserialize<TData>(document.Data)
            );
        }
    }
}
