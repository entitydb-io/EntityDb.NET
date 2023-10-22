using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.MongoDb.Documents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.MongoDb.Snapshots.Sessions;

internal record MongoSession
(
    ILogger<MongoSession> Logger,
    IMongoDatabase MongoDatabase,
    IClientSessionHandle ClientSessionHandle,
    MongoDbSnapshotSessionOptions Options
) : DisposableResourceBaseRecord, IMongoSession
{
    public string CollectionName => Options.CollectionName;
    
    private static readonly WriteConcern WriteConcern = WriteConcern.WMajority;

    private static readonly FilterDefinitionBuilder<SnapshotDocument> Filter = Builders<SnapshotDocument>.Filter;

    private static readonly ReplaceOptions UpsertOptions = new() { IsUpsert = true };

    private static FilterDefinition<SnapshotDocument> GetFilter(Id pointerId, VersionNumber pointerVersionNumber)
    {
        return Filter.And
        (
            Filter.Eq(document => document.PointerId, pointerId),
            Filter.Eq(document => document.PointerVersionNumber, pointerVersionNumber)
        );
    }
    
    public async Task Upsert(SnapshotDocument snapshotDocument, CancellationToken cancellationToken)
    {
        AssertNotReadOnly();

        var serverSessionId = ClientSessionHandle.ServerSession.Id.ToString();

        Logger
            .LogInformation
            (
                "Started Running MongoDb Upsert on `{DatabaseNamespace}.{CollectionName}`\n\nServer Session Id: {ServerSessionId}",
                MongoDatabase.DatabaseNamespace,
                Options.CollectionName,
                serverSessionId
            );

        await MongoDatabase
            .GetCollection<SnapshotDocument>(Options.CollectionName)
            .ReplaceOneAsync
            (
                ClientSessionHandle,
                GetFilter(snapshotDocument.PointerId, snapshotDocument.PointerVersionNumber),
                snapshotDocument,
                UpsertOptions,
                cancellationToken: cancellationToken
            );

        Logger
            .LogInformation
            (
                "Finished Running MongoDb Upsert on `{DatabaseNamespace}.{CollectionName}`\n\nServer Session Id: {ServerSessionId}",
                MongoDatabase.DatabaseNamespace,
                Options.CollectionName,
                serverSessionId
            );
    }

    public async Task<SnapshotDocument?> Find
    (
        Pointer snapshotPointer,
        CancellationToken cancellationToken
    )
    {
        var filter = GetFilter(snapshotPointer.Id, snapshotPointer.VersionNumber);

        var find = MongoDatabase
            .GetCollection<SnapshotDocument>(Options.CollectionName)
            .WithReadPreference(GetReadPreference())
            .WithReadConcern(GetReadConcern())
            .Find(ClientSessionHandle, filter);

        var query = find.ToString();
        var serverSessionId = ClientSessionHandle.ServerSession.Id.ToString();

        Logger
            .LogInformation
            (
                "Started MongoDb Query on `{DatabaseNamespace}.{CollectionName}`\n\nServer Session Id: {ServerSessionId}\n\nQuery: {Query}",
                MongoDatabase.DatabaseNamespace,
                Options.CollectionName,
                serverSessionId,
                query
            );

        var snapshotDocument = await find.SingleOrDefaultAsync(cancellationToken);

        Logger
            .LogInformation
            (
                "Finished MongoDb Query on `{DatabaseNamespace}.{CollectionName}`\n\nServer Session Id: {ServerSessionId}\n\nQuery: {Query}\n\nDocument Returned: {DocumentReturned}",
                MongoDatabase.DatabaseNamespace,
                Options.CollectionName,
                serverSessionId,
                query,
                snapshotDocument != null
            );

        return snapshotDocument;
    }
    
    public async Task Delete(Pointer[] snapshotPointers, CancellationToken cancellationToken)
    {
        AssertNotReadOnly();
        
        var filter = Filter.And(snapshotPointers.Select(pointer => GetFilter(pointer.Id, pointer.VersionNumber)));

        var serverSessionId = ClientSessionHandle.ServerSession.Id.ToString();
        
        var command = MongoDatabase
            .GetCollection<SnapshotDocument>(Options.CollectionName)
            .Find(filter)
            .ToString()!.Replace("find", "deleteMany");

        Logger
            .LogInformation
            (
                "Started Running MongoDb Delete on `{DatabaseNamespace}.{CollectionName}`\n\nServer SessionId: {ServerSessionId}\n\nCommand: {Command}",
                MongoDatabase.DatabaseNamespace,
                Options.CollectionName,
                serverSessionId,
                command
            );

        var deleteResult = await MongoDatabase
            .GetCollection<SnapshotDocument>(Options.CollectionName)
            .DeleteManyAsync
            (
                ClientSessionHandle,
                filter,
                cancellationToken: cancellationToken
            );

        Logger
            .LogInformation(
                "Finished Running MongoDb Delete on `{DatabaseNamespace}.{CollectionName}`\n\nServer SessionId: {ServerSessionId}\n\nCommand: {Command}\n\nDocuments Deleted: {DocumentsDeleted}",
                MongoDatabase.DatabaseNamespace,
                Options.CollectionName,
                serverSessionId,
                command,
                deleteResult.IsAcknowledged ? "(Not Available)" : deleteResult.DeletedCount
            );
    }

    public IMongoSession WithSessionOptions(MongoDbSnapshotSessionOptions options)
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

    [ExcludeFromCodeCoverage(Justification = "Tests should always run in a transaction.")]
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
        MongoDbSnapshotSessionOptions options
    )
    {
        return ActivatorUtilities.CreateInstance<MongoSession>(serviceProvider, mongoDatabase, clientSessionHandle,
            options);
    }
}
