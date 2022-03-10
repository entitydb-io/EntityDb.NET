using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Extensions;
using EntityDb.Common.Transactions;
using EntityDb.MongoDb.Serializers;
using EntityDb.MongoDb.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Transactions;

internal class MongoDbTransactionRepositoryFactory : DisposableResourceBaseClass, IMongoDbTransactionRepositoryFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnvelopeService<BsonDocument> _envelopeService;
    private readonly IOptionsFactory<TransactionSessionOptions> _optionsFactory;
    private readonly string _connectionString;
    private readonly string _databaseName;

    static MongoDbTransactionRepositoryFactory()
    {
        BsonSerializer.RegisterSerializer(new IdSerializer());
        BsonSerializer.RegisterSerializer(new TimeStampSerializer());
        BsonSerializer.RegisterSerializer(new VersionNumberSerializer());
    }
    
    public MongoDbTransactionRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IOptionsFactory<TransactionSessionOptions> optionsFactory,
        IEnvelopeService<BsonDocument> envelopeService,
        string connectionString,
        string databaseName
    )
    {
        _serviceProvider = serviceProvider;
        _optionsFactory = optionsFactory;
        _envelopeService = envelopeService;
        _connectionString = connectionString;
        _databaseName = databaseName;
    }

    public TransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName)
    {
        return _optionsFactory.Create(transactionSessionOptionsName);
    }

    public async Task<IMongoSession> CreateSession(TransactionSessionOptions transactionSessionOptions)
    {
        var mongoClient = new MongoClient(_connectionString);

        var mongoDatabase = mongoClient.GetDatabase(_databaseName);

        var clientSessionHandle = await mongoClient.StartSessionAsync(new ClientSessionOptions
        {
            CausalConsistency = true
        });

        return MongoSession.Create
        (
            _serviceProvider,
            mongoDatabase,
            clientSessionHandle,
            transactionSessionOptions
        );
    }

    public ITransactionRepository CreateRepository
    (
        IMongoSession mongoSession
    )
    {
        var mongoDbTransactionRepository = new MongoDbTransactionRepository
        (
            mongoSession,
            _envelopeService
        );

        return TryCatchTransactionRepository.Create(_serviceProvider, mongoDbTransactionRepository);
    }

    public static MongoDbTransactionRepositoryFactory Create(IServiceProvider serviceProvider,
        string connectionString, string databaseName)
    {
        return ActivatorUtilities.CreateInstance<MongoDbTransactionRepositoryFactory>
        (
            serviceProvider,
            connectionString,
            databaseName
        );
    }
}
