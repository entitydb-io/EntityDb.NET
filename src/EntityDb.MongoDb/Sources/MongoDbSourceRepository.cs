using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Annotations;
using EntityDb.Abstractions.Sources.Queries;
using EntityDb.Abstractions.States.Attributes;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Exceptions;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Documents.Queries;
using EntityDb.MongoDb.Sources.Sessions;
using MongoDB.Bson;
using Version = EntityDb.Abstractions.ValueObjects.Version;

namespace EntityDb.MongoDb.Sources;

internal class MongoDbSourceRepository : DisposableResourceBaseClass, ISourceRepository
{
    private readonly IEnvelopeService<BsonDocument> _envelopeService;
    private readonly IMongoSession _mongoSession;

    public MongoDbSourceRepository
    (
        IMongoSession mongoSession,
        IEnvelopeService<BsonDocument> envelopeService
    )
    {
        _mongoSession = mongoSession;
        _envelopeService = envelopeService;
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ISourceDataQuery sourceDataQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(sourceDataQuery)
            .EnumerateSourceIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(IMessageDataQuery messageDataQuery,
        CancellationToken cancellationToken = default)
    {
        return DeltaDataDocument
            .GetQuery(messageDataQuery)
            .EnumerateSourceIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDataDocument
            .GetQuery(leaseQuery)
            .EnumerateSourceIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDataDocument
            .GetQuery(tagQuery)
            .EnumerateSourceIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(ISourceDataQuery sourceDataQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(sourceDataQuery)
            .EnumerateSourceDataStatePointers(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(IMessageDataQuery messageDataQuery,
        CancellationToken cancellationToken = default)
    {
        return DeltaDataDocument
            .GetQuery(messageDataQuery)
            .EnumerateMessageStatePointers(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDataDocument
            .GetQuery(leaseQuery)
            .EnumerateMessageStatePointers(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDataDocument
            .GetQuery(tagQuery)
            .EnumerateMessageStatePointers(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<object> EnumerateAgentSignatures(ISourceDataQuery sourceDataQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(sourceDataQuery)
            .EnumerateData<AgentSignatureDocument, object>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<object> EnumerateDeltas(IMessageDataQuery messageDataQuery,
        CancellationToken cancellationToken = default)
    {
        return DeltaDataDocument
            .GetQuery(messageDataQuery)
            .EnumerateData<DeltaDataDocument, object>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<ILease> EnumerateLeases(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDataDocument
            .GetQuery(leaseQuery)
            .EnumerateData<LeaseDataDocument, ILease>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<ITag> EnumerateTags(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDataDocument
            .GetQuery(tagQuery)
            .EnumerateData<TagDataDocument, ITag>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<IAnnotatedSourceData<object>> EnumerateAnnotatedAgentSignatures(
        ISourceDataQuery sourceDataQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(sourceDataQuery)
            .EnumerateEntitiesAnnotation<AgentSignatureDocument, object>(_mongoSession, _envelopeService,
                cancellationToken);
    }

    public IAsyncEnumerable<IAnnotatedMessageData<object>> EnumerateAnnotatedDeltas(IMessageDataQuery messageDataQuery,
        CancellationToken cancellationToken = default)
    {
        return DeltaDataDocument
            .GetQuery(messageDataQuery)
            .EnumerateAnnotatedSourceData<DeltaDataDocument,
                object>(_mongoSession, _envelopeService, cancellationToken);
    }

    public async Task<bool> Commit(Source source,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _mongoSession.StartTransaction();

            await PutAgentSignature(source, cancellationToken);

            foreach (var message in source.Messages)
            {
                var currentMessage = message;

                cancellationToken.ThrowIfCancellationRequested();

                var previousVersion = await DeltaDataDocument
                    .GetLastStateVersion(_mongoSession, message.StatePointer.Id, cancellationToken);

                if (message.StatePointer.Version == Version.Zero)
                {
                    currentMessage = currentMessage with
                    {
                        StatePointer = currentMessage.StatePointer.Id + previousVersion.Next(),
                    };
                }
                else
                {
                    OptimisticConcurrencyException.ThrowIfMismatch(previousVersion.Next(),
                        message.StatePointer.Version);
                }

                await Put(source, currentMessage, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();

            await _mongoSession.CommitTransaction(cancellationToken);

            return true;
        }
        catch
        {
            await _mongoSession.AbortTransaction();

            throw;
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await _mongoSession.DisposeAsync();
    }

    private async Task PutAgentSignature(Source source, CancellationToken cancellationToken)
    {
        await AgentSignatureDocument
            .GetInsertCommand(_envelopeService, source)
            .Execute(_mongoSession, cancellationToken);
    }

    private async Task Put(Source source, Message message, CancellationToken cancellationToken)
    {
        await DeltaDataDocument
            .GetInsertCommand(_envelopeService, source, message)
            .Execute(_mongoSession, cancellationToken);

        if (message.AddLeases.Length > 0)
        {
            await LeaseDataDocument
                .GetInsertCommand(_envelopeService, source, message)
                .Execute(_mongoSession, cancellationToken);
        }

        if (message.AddTags.Length > 0)
        {
            await TagDataDocument
                .GetInsertCommand(_envelopeService, source, message)
                .Execute(_mongoSession, cancellationToken);
        }

        if (message.DeleteLeases.Length > 0)
        {
            await LeaseDataDocument
                .GetDeleteCommand(message)
                .Execute(_mongoSession, cancellationToken);
        }

        if (message.DeleteTags.Length > 0)
        {
            await TagDataDocument
                .GetDeleteCommand(message)
                .Execute(_mongoSession, cancellationToken);
        }
    }
}
