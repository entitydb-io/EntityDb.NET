using EntityDb.Abstractions.Sources;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Sources;
using EntityDb.MongoDb.Sources.Sessions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources;

internal class MongoDbSourceRepositoryFactory : DisposableResourceBaseClass, IMongoDbSourceRepositoryFactory
{
    private readonly IEnvelopeService<BsonDocument> _envelopeService;
    private readonly IOptionsFactory<MongoDbSourceSessionOptions> _optionsFactory;
    private readonly IServiceProvider _serviceProvider;

    public MongoDbSourceRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IOptionsFactory<MongoDbSourceSessionOptions> optionsFactory,
        IEnvelopeService<BsonDocument> envelopeService
    )
    {
        _serviceProvider = serviceProvider;
        _optionsFactory = optionsFactory;
        _envelopeService = envelopeService;
    }

    public MongoDbSourceSessionOptions GetSourceSessionOptions(string sourceSessionOptionsName)
    {
        return _optionsFactory.Create(sourceSessionOptionsName);
    }

    public async Task<IMongoSession> CreateSession(MongoDbSourceSessionOptions options,
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

    public ISourceRepository CreateRepository
    (
        IMongoSession mongoSession
    )
    {
        ISourceRepository sourceRepository = new MongoDbSourceRepository
        (
            mongoSession,
            _envelopeService
        );

        sourceRepository = TryCatchSourceRepository.Create(_serviceProvider, sourceRepository);

        sourceRepository = PublishSourceRepository.Create(_serviceProvider, sourceRepository);
        
        return sourceRepository;
    }
}
