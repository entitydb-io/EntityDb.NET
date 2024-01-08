using EntityDb.Abstractions.Sources;
using EntityDb.MongoDb.Sources.Sessions;

namespace EntityDb.MongoDb.Sources;

internal interface IMongoDbSourceRepositoryFactory : ISourceRepositoryFactory
{
    async Task<ISourceRepository> ISourceRepositoryFactory.CreateRepository(
        string sourceSessionOptionsName, CancellationToken cancellationToken)
    {
        var options = GetSourceSessionOptions(sourceSessionOptionsName);

        var mongoSession = await CreateSession(options, cancellationToken);

        return CreateRepository(mongoSession);
    }

    MongoDbSourceSessionOptions GetSourceSessionOptions(string sourceSessionOptionsName);

    Task<IMongoSession> CreateSession(MongoDbSourceSessionOptions options,
        CancellationToken cancellationToken);

    ISourceRepository CreateRepository(IMongoSession mongoSession);
}
