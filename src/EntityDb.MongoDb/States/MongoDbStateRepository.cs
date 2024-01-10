using EntityDb.Abstractions.States;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.States.Sessions;
using MongoDB.Bson;

namespace EntityDb.MongoDb.States;

internal class MongoDbStateRepository<TState> : DisposableResourceBaseClass, IStateRepository<TState>
    where TState : notnull
{
    private readonly IEnvelopeService<BsonDocument> _envelopeService;
    private readonly IMongoSession _mongoSession;

    public MongoDbStateRepository
    (
        IEnvelopeService<BsonDocument> envelopeService,
        IMongoSession mongoSession
    )
    {
        _envelopeService = envelopeService;
        _mongoSession = mongoSession;
    }

    public async Task<bool> Put(Pointer statePointer, TState state,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _mongoSession.StartTransaction();

            await _mongoSession.Upsert(new StateDocument
            {
                DataType = state.GetType().Name,
                Data = _envelopeService.Serialize(state),
                StateId = statePointer.Id,
                StateVersion = statePointer.Version,
                StatePointer = statePointer,
            }, cancellationToken);

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

    public async Task<TState?> Get(Pointer statePointer,
        CancellationToken cancellationToken = default)
    {
        var stateDocument = await _mongoSession.Fetch(statePointer, cancellationToken);

        if (stateDocument == null)
        {
            return default;
        }

        return _envelopeService
            .Deserialize<TState>(stateDocument.Data);
    }

    public async Task<bool> Delete(Pointer[] statePointers, CancellationToken cancellationToken = default)
    {
        await _mongoSession.Delete(statePointers, cancellationToken);

        return true;
    }

    public override async ValueTask DisposeAsync()
    {
        await _mongoSession.DisposeAsync();
    }
}
