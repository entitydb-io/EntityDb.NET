using EntityDb.Abstractions.States;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.States;
using EntityDb.MongoDb.States.Sessions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.States;

internal sealed class MongoDbStateRepositoryFactory<TState> : DisposableResourceBaseClass,
    IMongoDbStateRepositoryFactory<TState>
    where TState : notnull
{
    private readonly IEnvelopeService<BsonDocument> _envelopeService;
    private readonly IOptionsFactory<MongoDbStateSessionOptions> _optionsFactory;
    private readonly IServiceProvider _serviceProvider;

    public MongoDbStateRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IOptionsFactory<MongoDbStateSessionOptions> optionsFactory,
        IEnvelopeService<BsonDocument> envelopeService
    )
    {
        _serviceProvider = serviceProvider;
        _optionsFactory = optionsFactory;
        _envelopeService = envelopeService;
    }

    public MongoDbStateSessionOptions GetSessionOptions(string stateSessionOptionsName)
    {
        return _optionsFactory.Create(stateSessionOptionsName);
    }

    public async Task<IMongoSession> CreateSession(MongoDbStateSessionOptions options,
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

    public IStateRepository<TState> CreateRepository
    (
        IMongoSession mongoSession
    )
    {
        var mongoDbStateRepository = new MongoDbStateRepository<TState>
        (
            _envelopeService,
            mongoSession
        );

        return TryCatchStateRepository<TState>.Create(_serviceProvider, mongoDbStateRepository);
    }
}
