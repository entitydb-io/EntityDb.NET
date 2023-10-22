using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Snapshots.Sessions;
using MongoDB.Bson;

namespace EntityDb.MongoDb.Snapshots;

internal class MongoDbSnapshotRepository<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepository<TSnapshot>
    where TSnapshot : notnull
{
    private readonly IEnvelopeService<BsonDocument> _envelopeService;
    private readonly IMongoSession _mongoSession;

    public MongoDbSnapshotRepository
    (
        IEnvelopeService<BsonDocument> envelopeService,
        IMongoSession mongoSession
    )
    {
        _envelopeService = envelopeService;
        _mongoSession = mongoSession;
    }

    public async Task<bool> PutSnapshot(Pointer snapshotPointer, TSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _mongoSession.StartTransaction();

            await _mongoSession.Upsert(new SnapshotDocument
            {
                DataType = snapshot.GetType().Name,
                Data = _envelopeService.Serialize(snapshot),
                PointerId = snapshotPointer.Id,
                PointerVersionNumber = snapshotPointer.VersionNumber,
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

    public async Task<TSnapshot?> GetSnapshotOrDefault(Pointer snapshotPointer,
        CancellationToken cancellationToken = default)
    {
        var snapshotDocument = await _mongoSession.Find(snapshotPointer, cancellationToken);

        if (snapshotDocument == null)
        {
            return default;
        }

        return _envelopeService
            .Deserialize<TSnapshot>(snapshotDocument.Data);
    }

    public async Task<bool> DeleteSnapshots(Pointer[] snapshotPointers, CancellationToken cancellationToken = default)
    {
        await _mongoSession.Delete(snapshotPointers, cancellationToken);

        return true;
    }

    public override async ValueTask DisposeAsync()
    {
        await _mongoSession.DisposeAsync();
    }
}
