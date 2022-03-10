using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions;

internal record MongoSession
(
    ILogger<MongoSession> Logger,
    IMongoDatabase MongoDatabase,
    IClientSessionHandle ClientSessionHandle,
    TransactionSessionOptions TransactionSessionOptions
) : DisposableResourceBaseRecord, IMongoSession
{
    private static readonly WriteConcern WriteConcern = WriteConcern.WMajority;

    private ReadPreference GetReadPreference()
    {
        if (!TransactionSessionOptions.ReadOnly)
        {
            return ReadPreference.Primary;
        }

        return TransactionSessionOptions.SecondaryPreferred
            ? ReadPreference.SecondaryPreferred
            : ReadPreference.PrimaryPreferred;
    }

    [ExcludeFromCodeCoverage(Justification = "Tests should always run in a transaction.")]
    private ReadConcern GetReadConcern()
    {
        return ClientSessionHandle.IsInTransaction
            ? ReadConcern.Snapshot
            : ReadConcern.Majority;
    }

    private void AssertNotReadOnly()
    {
        if (TransactionSessionOptions.ReadOnly)
        {
            throw new CannotWriteInReadOnlyModeException();
        }
    }

    public async Task Insert<TDocument>(string collectionName, TDocument[] bsonDocuments)
    {
        AssertNotReadOnly();

        Logger
            .LogInformation
            (
                "Running MongoDb Insert on `{DatabaseNamespace}.{CollectionName}`\n\nServer Session Id: {ServerSessionId}",
                MongoDatabase.DatabaseNamespace,
                collectionName,
                ClientSessionHandle.ServerSession.Id.ToString()
            );
        
        await MongoDatabase
            .GetCollection<TDocument>(collectionName)
            .InsertManyAsync
            (
                ClientSessionHandle,
                bsonDocuments
            );
    }

    public Task<List<TDocument>> Find<TDocument>
    (
        string collectionName,
        FilterDefinition<BsonDocument> filter,
        ProjectionDefinition<BsonDocument, TDocument> projection,
        SortDefinition<BsonDocument>? sort,
        int? skip,
        int? limit
    )
    {
        var find = MongoDatabase
            .GetCollection<BsonDocument>(collectionName)
            .WithReadPreference(GetReadPreference())
            .WithReadConcern(GetReadConcern())
            .Find(ClientSessionHandle, filter, new FindOptions
            {
                MaxTime = TransactionSessionOptions.ReadTimeout
            })
            .Project(projection);

        if (sort != null)
        {
            find = find.Sort(sort);
        }

        if (skip != null)
        {
            find = find.Skip(skip);
        }

        if (limit != null)
        {
            find = find.Limit(limit);
        }
        
        Logger
            .LogInformation
            (
                "Running MongoDb Query on `{DatabaseNamespace}.{CollectionName}`\n\nServer Session Id: {ServerSessionId}\n\nQuery: {Query}",
                MongoDatabase.DatabaseNamespace,
                collectionName,
                ClientSessionHandle.ServerSession.Id.ToString(),
                find.ToString()
            );

        return find.ToListAsync();
    }

    public async Task Delete<TDocument>(string collectionName,
        FilterDefinition<TDocument> filterDefinition)
    {
        AssertNotReadOnly();

        Logger
            .LogInformation
            (
                "Running MongoDb Delete on `{DatabaseNamespace}.{CollectionName}`\n\nServer SessionId: {ServerSessionId}\n\nCommand: {Command}",
                MongoDatabase.DatabaseNamespace,
                collectionName,
                ClientSessionHandle.ServerSession.Id.ToString(),
                MongoDatabase.GetCollection<TDocument>(collectionName).Find(filterDefinition).ToString()!.Replace("find", "deleteMany")
            );
        
        await MongoDatabase
            .GetCollection<TDocument>(collectionName)
            .DeleteManyAsync
            (
                ClientSessionHandle,
                filterDefinition
            );
    }

    public IMongoSession WithTransactionSessionOptions(TransactionSessionOptions transactionSessionOptions)
    {
        return this with
        {
            TransactionSessionOptions = transactionSessionOptions
        };
    }

    public void StartTransaction()
    {
        AssertNotReadOnly();

        ClientSessionHandle.StartTransaction(new TransactionOptions
        (
            writeConcern: WriteConcern,
            maxCommitTime: TransactionSessionOptions.WriteTimeout
        ));
    }

    [ExcludeFromCodeCoverage(Justification = "Tests should run with the Debug configuration, and should not execute this method.")]
    public async Task CommitTransaction()
    {
        AssertNotReadOnly();

        await ClientSessionHandle.CommitTransactionAsync();
    }

    public async Task AbortTransaction()
    {
        AssertNotReadOnly();

        await ClientSessionHandle.AbortTransactionAsync();
    }

    public override ValueTask DisposeAsync()
    {
        ClientSessionHandle.Dispose();

        return ValueTask.CompletedTask;
    }

    public static IMongoSession Create
    (
        IServiceProvider serviceProvider,
        IMongoDatabase mongoDatabase,
        IClientSessionHandle clientSessionHandle,
        TransactionSessionOptions transactionSessionOptions
    )
    {
        return ActivatorUtilities.CreateInstance<MongoSession>(serviceProvider, mongoDatabase, clientSessionHandle, transactionSessionOptions);
    }
}
