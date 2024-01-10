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

    public IAsyncEnumerable<Id> EnumerateSourceIds(ISourceDataDataQuery sourceDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(sourceDataDataQuery)
            .EnumerateSourceIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(IMessageDataDataQuery messageDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return DeltaDataDocument
            .GetQuery(messageDataDataQuery)
            .EnumerateSourceIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ILeaseDataDataQuery leaseDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDataDocument
            .GetQuery(leaseDataDataQuery)
            .EnumerateSourceIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ITagDataDataQuery tagDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDataDocument
            .GetQuery(tagDataDataQuery)
            .EnumerateSourceIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(ISourceDataDataQuery sourceDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(sourceDataDataQuery)
            .EnumerateSourceDataStatePointers(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(IMessageDataDataQuery messageDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return DeltaDataDocument
            .GetQuery(messageDataDataQuery)
            .EnumerateMessageStatePointers(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(ILeaseDataDataQuery leaseDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDataDocument
            .GetQuery(leaseDataDataQuery)
            .EnumerateMessageStatePointers(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Pointer> EnumerateStatePointers(ITagDataDataQuery tagDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDataDocument
            .GetQuery(tagDataDataQuery)
            .EnumerateMessageStatePointers(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<object> EnumerateAgentSignatures(ISourceDataDataQuery sourceDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(sourceDataDataQuery)
            .EnumerateData<AgentSignatureDocument, object>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<object> EnumerateDeltas(IMessageDataDataQuery messageDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return DeltaDataDocument
            .GetQuery(messageDataDataQuery)
            .EnumerateData<DeltaDataDocument, object>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<ILease> EnumerateLeases(ILeaseDataDataQuery leaseDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDataDocument
            .GetQuery(leaseDataDataQuery)
            .EnumerateData<LeaseDataDocument, ILease>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<ITag> EnumerateTags(ITagDataDataQuery tagDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDataDocument
            .GetQuery(tagDataDataQuery)
            .EnumerateData<TagDataDocument, ITag>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<IAnnotatedSourceData<object>> EnumerateAnnotatedAgentSignatures(
        ISourceDataDataQuery sourceDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(sourceDataDataQuery)
            .EnumerateEntitiesAnnotation<AgentSignatureDocument, object>(_mongoSession, _envelopeService,
                cancellationToken);
    }

    public IAsyncEnumerable<IAnnotatedMessageData<object>> EnumerateAnnotatedDeltas(IMessageDataDataQuery messageDataDataQuery,
        CancellationToken cancellationToken = default)
    {
        return DeltaDataDocument
            .GetQuery(messageDataDataQuery)
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
