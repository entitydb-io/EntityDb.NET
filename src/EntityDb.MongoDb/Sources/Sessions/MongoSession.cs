using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.MongoDb.Sources.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EntityDb.MongoDb.Sources.Sessions;

internal record MongoSession
(
    ILogger<MongoSession> Logger,
    IMongoDatabase MongoDatabase,
    IClientSessionHandle ClientSessionHandle,
    MongoDbSourceSessionOptions Options
) : DisposableResourceBaseRecord, IMongoSession
{
    private static readonly WriteConcern WriteConcern = WriteConcern.WMajority;

    public async Task Insert<TDocument>(string collectionName, TDocument[] bsonDocuments,
        CancellationToken cancellationToken)
    {
        AssertNotReadOnly();

        var serverSessionId = ClientSessionHandle.ServerSession.Id.ToString();

        Logger
            .LogInformation
            (
                "Started Running MongoDb Insert on `{DatabaseNamespace}.{CollectionName}`\n\nServer Session Id: {ServerSessionId}\n\nDocuments Committed: {DocumentsCommitted}",
                MongoDatabase.DatabaseNamespace,
                collectionName,
                serverSessionId,
                bsonDocuments.Length
            );

        await MongoDatabase
            .GetCollection<TDocument>(collectionName)
            .InsertManyAsync
            (
                ClientSessionHandle,
                bsonDocuments,
                cancellationToken: cancellationToken
            );

        Logger
            .LogInformation
            (
                "Finished Running MongoDb Insert on `{DatabaseNamespace}.{CollectionName}`\n\nServer Session Id: {ServerSessionId}\n\nDocuments Committed: {DocumentsCommitted}",
                MongoDatabase.DatabaseNamespace,
                collectionName,
                serverSessionId,
                bsonDocuments.Length
            );
    }

    public async IAsyncEnumerable<TDocument> Find<TDocument>
    (
        string collectionName,
        FilterDefinition<BsonDocument> filter,
        ProjectionDefinition<BsonDocument, TDocument> projection,
        SortDefinition<BsonDocument>? sort,
        int? skip,
        int? limit,
        MongoDbQueryOptions? options,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var find = MongoDatabase
            .GetCollection<BsonDocument>(collectionName)
            .WithReadPreference(GetReadPreference())
            .WithReadConcern(GetReadConcern())
            .Find(ClientSessionHandle, filter, options?.FindOptions)
            .Project(projection);

        if (sort is not null)
        {
            find = find.Sort(sort);
        }

        if (skip is not null)
        {
            find = find.Skip(skip);
        }

        if (limit is not null)
        {
            find = find.Limit(limit);
        }

        var query = find.ToString();
        var serverSessionId = ClientSessionHandle.ServerSession.Id.ToString();

        Logger
            .LogInformation
            (
                "Started Enumerating MongoDb Query on `{DatabaseNamespace}.{CollectionName}`\n\nServer Session Id: {ServerSessionId}\n\nQuery: {Query}",
                MongoDatabase.DatabaseNamespace,
                collectionName,
                serverSessionId,
                query
            );

        ulong documentCount = 0;

        using var cursor = await find.ToCursorAsync(cancellationToken);

        while (await cursor.MoveNextAsync(cancellationToken))
        {
            foreach (var document in cursor.Current)
            {
                documentCount += 1;

                yield return document;
            }
        }

        Logger
            .LogInformation
            (
                "Finished Enumerating MongoDb Query on `{DatabaseNamespace}.{CollectionName}`\n\nServer Session Id: {ServerSessionId}\n\nQuery: {Query}\n\nDocuments Returned: {DocumentsReturned}",
                MongoDatabase.DatabaseNamespace,
                collectionName,
                serverSessionId,
                query,
                documentCount
            );
    }


    public async Task Delete<TDocument>(string collectionName,
        FilterDefinition<TDocument> filterDefinition, CancellationToken cancellationToken)
    {
        AssertNotReadOnly();

        var serverSessionId = ClientSessionHandle.ServerSession.Id.ToString();
        var command =
            MongoDatabase.GetCollection<TDocument>(collectionName).Find(filterDefinition).ToString()!.Replace("find",
                "deleteMany");

        Logger
            .LogInformation
            (
                "Started Running MongoDb Delete on `{DatabaseNamespace}.{CollectionName}`\n\nServer SessionId: {ServerSessionId}\n\nCommand: {Command}",
                MongoDatabase.DatabaseNamespace,
                collectionName,
                serverSessionId,
                command
            );

        var deleteResult = await MongoDatabase
            .GetCollection<TDocument>(collectionName)
            .DeleteManyAsync
            (
                ClientSessionHandle,
                filterDefinition,
                cancellationToken: cancellationToken
            );

        Logger
            .LogInformation(
                "Finished Running MongoDb Delete on `{DatabaseNamespace}.{CollectionName}`\n\nServer SessionId: {ServerSessionId}\n\nCommand: {Command}\n\nDocuments Deleted: {DocumentsDeleted}",
                MongoDatabase.DatabaseNamespace,
                collectionName,
                serverSessionId,
                command,
                deleteResult.IsAcknowledged ? "(Not Available)" : deleteResult.DeletedCount
            );
    }

    public IMongoSession WithSourceSessionOptions(MongoDbSourceSessionOptions options)
    {
        return this with { Options = options };
    }

    public void StartTransaction()
    {
        AssertNotReadOnly();

        ClientSessionHandle.StartTransaction(new TransactionOptions
        (
            writeConcern: WriteConcern,
            maxCommitTime: Options.WriteTimeout
        ));

        Logger
            .LogInformation
            (
                "Started MongoDb Transaction on `{DatabaseNamespace}`\n\nServer SessionId: {ServerSessionId}",
                MongoDatabase.DatabaseNamespace,
                ClientSessionHandle.ServerSession.Id.ToString()
            );
    }

    [ExcludeFromCodeCoverage(Justification =
        "Tests should run with the Debug configuration, and should not execute this method.")]
    public async Task CommitTransaction(CancellationToken cancellationToken)
    {
        AssertNotReadOnly();

        await ClientSessionHandle.CommitTransactionAsync(cancellationToken);

        Logger
            .LogInformation
            (
                "Committed MongoDb Transaction on `{DatabaseNamespace}`\n\nServer SessionId: {ServerSessionId}",
                MongoDatabase.DatabaseNamespace,
                ClientSessionHandle.ServerSession.Id.ToString()
            );
    }

    public async Task AbortTransaction()
    {
        AssertNotReadOnly();

        await ClientSessionHandle.AbortTransactionAsync();

        Logger
            .LogInformation
            (
                "Aborted MongoDb Transaction on `{DatabaseNamespace}`\n\nServer SessionId: {ServerSessionId}",
                MongoDatabase.DatabaseNamespace,
                ClientSessionHandle.ServerSession.Id.ToString()
            );
    }

    public override ValueTask DisposeAsync()
    {
        ClientSessionHandle.Dispose();

        return base.DisposeAsync();
    }

    private ReadPreference GetReadPreference()
    {
        if (!Options.ReadOnly)
        {
            return ReadPreference.Primary;
        }

        return Options.SecondaryPreferred
            ? ReadPreference.SecondaryPreferred
            : ReadPreference.PrimaryPreferred;
    }

    [ExcludeFromCodeCoverage(Justification = "Tests should always run in a source.")]
    private ReadConcern GetReadConcern()
    {
        return ClientSessionHandle.IsInTransaction
            ? ReadConcern.Snapshot
            : ReadConcern.Majority;
    }

    private void AssertNotReadOnly()
    {
        if (Options.ReadOnly)
        {
            throw new CannotWriteInReadOnlyModeException();
        }
    }

    public static IMongoSession Create
    (
        IServiceProvider serviceProvider,
        IMongoDatabase mongoDatabase,
        IClientSessionHandle clientSessionHandle,
        MongoDbSourceSessionOptions options
    )
    {
        return ActivatorUtilities.CreateInstance<MongoSession>(serviceProvider, mongoDatabase, clientSessionHandle,
            options);
    }
}
