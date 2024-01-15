using EntityDb.Abstractions.States;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.MongoDb.Documents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.MongoDb.States.Sessions;

internal sealed record MongoSession
(
    ILogger<MongoSession> Logger,
    IMongoDatabase MongoDatabase,
    IClientSessionHandle ClientSessionHandle,
    MongoDbStateSessionOptions Options
) : DisposableResourceBaseRecord, IMongoSession
{
    private static readonly WriteConcern WriteConcern = WriteConcern.WMajority;

    private static readonly FilterDefinitionBuilder<StateDocument> Filter = Builders<StateDocument>.Filter;

    private static readonly ReplaceOptions UpsertOptions = new() { IsUpsert = true };
    public string CollectionName => Options.CollectionName;

    public async Task Upsert(StateDocument stateDocument, CancellationToken cancellationToken)
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
            .GetCollection<StateDocument>(Options.CollectionName)
            .ReplaceOneAsync
            (
                ClientSessionHandle,
                GetFilter(stateDocument.StatePointer),
                stateDocument,
                UpsertOptions,
                cancellationToken
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

    public async Task<StateDocument?> Fetch
    (
        StatePointer statePointer,
        CancellationToken cancellationToken
    )
    {
        var filter = GetFilter(statePointer);

        var find = MongoDatabase
            .GetCollection<StateDocument>(Options.CollectionName)
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

        var stateDocument = await find.SingleOrDefaultAsync(cancellationToken);

        Logger
            .LogInformation
            (
                "Finished MongoDb Query on `{DatabaseNamespace}.{CollectionName}`\n\nServer Session Id: {ServerSessionId}\n\nQuery: {Query}\n\nDocument Returned: {DocumentReturned}",
                MongoDatabase.DatabaseNamespace,
                Options.CollectionName,
                serverSessionId,
                query,
                stateDocument != null
            );

        return stateDocument;
    }

    public async Task Delete(StatePointer[] statePointer, CancellationToken cancellationToken)
    {
        AssertNotReadOnly();

        var filter = Filter.And(statePointer.Select(GetFilter));

        var serverSessionId = ClientSessionHandle.ServerSession.Id.ToString();

        var command = MongoDatabase
            .GetCollection<StateDocument>(Options.CollectionName)
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
            .GetCollection<StateDocument>(Options.CollectionName)
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

    public IMongoSession WithSessionOptions(MongoDbStateSessionOptions options)
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

    private static FilterDefinition<StateDocument> GetFilter(StatePointer statePointer)
    {
        return Filter.Eq(document => document.StatePointer, statePointer);
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
            throw new ReadOnlyWriteException();
        }
    }

    public static IMongoSession Create
    (
        IServiceProvider serviceProvider,
        IMongoDatabase mongoDatabase,
        IClientSessionHandle clientSessionHandle,
        MongoDbStateSessionOptions options
    )
    {
        return ActivatorUtilities.CreateInstance<MongoSession>(serviceProvider, mongoDatabase, clientSessionHandle,
            options);
    }
}
