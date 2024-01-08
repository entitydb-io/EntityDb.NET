using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Annotations;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.Sources.Queries;
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

    public IAsyncEnumerable<Id> EnumerateSourceIds(IMessageGroupQuery messageGroupQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(messageGroupQuery)
            .EnumerateSourceIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(IMessageQuery messageQuery,
        CancellationToken cancellationToken = default)
    {
        return DeltaDocument
            .GetQuery(messageQuery)
            .EnumerateSourceIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .EnumerateSourceIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateSourceIds(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .EnumerateSourceIds(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Pointer> EnumerateEntityPointers(IMessageGroupQuery messageGroupQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(messageGroupQuery)
            .EnumerateMessageGroupEntityPointers(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Pointer> EnumerateEntityPointers(IMessageQuery messageQuery,
        CancellationToken cancellationToken = default)
    {
        return DeltaDocument
            .GetQuery(messageQuery)
            .EnumerateMessageEntityPointers(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Pointer> EnumerateEntityPointers(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .EnumerateMessageEntityPointers(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<Pointer> EnumerateEntityPointers(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .EnumerateMessageEntityPointers(_mongoSession, cancellationToken);
    }

    public IAsyncEnumerable<object> EnumerateAgentSignatures(IMessageGroupQuery messageGroupQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(messageGroupQuery)
            .EnumerateData<AgentSignatureDocument, object>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<object> EnumerateDeltas(IMessageQuery messageQuery,
        CancellationToken cancellationToken = default)
    {
        return DeltaDocument
            .GetQuery(messageQuery)
            .EnumerateData<DeltaDocument, object>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<ILease> EnumerateLeases(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .EnumerateData<LeaseDocument, ILease>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<ITag> EnumerateTags(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .EnumerateData<TagDocument, ITag>(_mongoSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<IAnnotatedSourceGroupData<object>> EnumerateAnnotatedAgentSignatures(
        IMessageGroupQuery messageGroupQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(messageGroupQuery)
            .EnumerateEntitiesAnnotation<AgentSignatureDocument, object>(_mongoSession, _envelopeService,
                cancellationToken);
    }

    public IAsyncEnumerable<IAnnotatedSourceData<object>> EnumerateAnnotatedDeltas(IMessageQuery messageQuery,
        CancellationToken cancellationToken = default)
    {
        return DeltaDocument
            .GetQuery(messageQuery)
            .EnumerateEntityAnnotation<DeltaDocument, object>(_mongoSession, _envelopeService, cancellationToken);
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

                var previousVersion = await DeltaDocument
                    .GetLastEntityVersion(_mongoSession, message.EntityPointer.Id, cancellationToken);

                if (message.EntityPointer.Version == Version.Zero)
                {
                    currentMessage = currentMessage with
                    {
                        EntityPointer = currentMessage.EntityPointer.Id + previousVersion.Next(),
                    };
                }
                else
                {
                    OptimisticConcurrencyException.ThrowIfMismatch(previousVersion.Next(),
                        message.EntityPointer.Version);
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
        await DeltaDocument
            .GetInsertCommand(_envelopeService, source, message)
            .Execute(_mongoSession, cancellationToken);

        if (message.AddLeases.Length > 0)
        {
            await LeaseDocument
                .GetInsertCommand(_envelopeService, source, message)
                .Execute(_mongoSession, cancellationToken);
        }

        if (message.AddTags.Length > 0)
        {
            await TagDocument
                .GetInsertCommand(_envelopeService, source, message)
                .Execute(_mongoSession, cancellationToken);
        }

        if (message.DeleteLeases.Length > 0)
        {
            await LeaseDocument
                .GetDeleteCommand(message)
                .Execute(_mongoSession, cancellationToken);
        }

        if (message.DeleteTags.Length > 0)
        {
            await TagDocument
                .GetDeleteCommand(message)
                .Execute(_mongoSession, cancellationToken);
        }
    }
}
