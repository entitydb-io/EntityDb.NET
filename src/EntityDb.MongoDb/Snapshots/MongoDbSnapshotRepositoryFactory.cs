using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Snapshots;
using EntityDb.MongoDb.Snapshots.Sessions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Snapshots;

internal class MongoDbSnapshotRepositoryFactory<TSnapshot> : DisposableResourceBaseClass,
    IMongoDbSnapshotRepositoryFactory<TSnapshot>
    where TSnapshot : notnull
{
    private readonly IEnvelopeService<BsonDocument> _envelopeService;
    private readonly IOptionsFactory<MongoDbSnapshotSessionOptions> _optionsFactory;
    private readonly IServiceProvider _serviceProvider;

    public MongoDbSnapshotRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IOptionsFactory<MongoDbSnapshotSessionOptions> optionsFactory,
        IEnvelopeService<BsonDocument> envelopeService
    )
    {
        _serviceProvider = serviceProvider;
        _optionsFactory = optionsFactory;
        _envelopeService = envelopeService;
    }

    public MongoDbSnapshotSessionOptions GetSessionOptions(string snapshotSessionOptionsName)
    {
        return _optionsFactory.Create(snapshotSessionOptionsName);
    }

    public async Task<IMongoSession> CreateSession(MongoDbSnapshotSessionOptions options,
        CancellationToken cancellationToken)
    {
        var mongoClient = new MongoClient(options.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(options.DatabaseName);

        var clientSessionHandle =
            await mongoClient.StartSessionAsync(new ClientSessionOptions { CausalConsistency = true },
                cancellationToken);

        return MongoSession.Create
        (
            _serviceProvider,
            mongoDatabase,
            clientSessionHandle,
            options
        );
    }

    public ISnapshotRepository<TSnapshot> CreateRepository
    (
        IMongoSession mongoSession
    )
    {
        var mongoDbSnapshotRepository = new MongoDbSnapshotRepository<TSnapshot>
        (
            _envelopeService,
            mongoSession
        );

        return TryCatchSnapshotRepository<TSnapshot>.Create(_serviceProvider, mongoDbSnapshotRepository);
    }
}
