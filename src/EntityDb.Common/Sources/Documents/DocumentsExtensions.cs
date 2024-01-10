using EntityDb.Abstractions.Sources.Annotations;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Sources.Annotations;
using System.Runtime.CompilerServices;

namespace EntityDb.Common.Sources.Documents;

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

    public static IAsyncEnumerable<Pointer> EnumeratePointers<TDocument>
    (
        this IAsyncEnumerable<TDocument> documents,
        int? skip,
        int? limit,
        Func<IAsyncEnumerable<TDocument>, IAsyncEnumerable<Pointer>> mapToPointers
    )
    {
        var pointers = mapToPointers
            .Invoke(documents)
            .Distinct();

        if (skip.HasValue)
        {
            pointers = pointers.Skip(skip.Value);
        }

        if (limit.HasValue)
        {
            pointers = pointers.Take(limit.Value);
        }

        return pointers;
    }

    public static async IAsyncEnumerable<IAnnotatedMessageData<TData>> EnumerateAnnotatedSourceData<TDocument,
        TSerializedData, TData>
    (
        this IAsyncEnumerable<TDocument> documents,
        IEnvelopeService<TSerializedData> envelopeService,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
        where TDocument : IMessageDataDocument<TSerializedData>
        where TData : notnull
    {
        await using var enumerator = documents.GetAsyncEnumerator(cancellationToken);

        while (await enumerator.MoveNextAsync(cancellationToken))
        {
            var document = enumerator.Current;

            yield return AnnotatedMessageData<TData>.CreateFromBoxedData
            (
                document.SourceId,
                document.SourceTimeStamp,
                document.MessageId,
                envelopeService.Deserialize<TData>(document.Data),
                document.StatePointer
            );
        }
    }

    public static async IAsyncEnumerable<IAnnotatedSourceData<TData>> EnumerateEntitiesAnnotation<TDocument,
        TSerializedData, TData>
    (
        this IAsyncEnumerable<TDocument> documents,
        IEnvelopeService<TSerializedData> envelopeService,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
        where TDocument : ISourceDataDocument<TSerializedData>
        where TData : notnull
    {
        await using var enumerator = documents.GetAsyncEnumerator(cancellationToken);

        while (await enumerator.MoveNextAsync(cancellationToken))
        {
            var document = enumerator.Current;

            yield return AnnotatedSourceData<TData>.CreateFromBoxedData
            (
                document.SourceId,
                document.SourceTimeStamp,
                document.MessageIds,
                envelopeService.Deserialize<TData>(document.Data),
                document.StatePointers
            );
        }
    }
}
