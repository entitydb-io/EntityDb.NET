using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Common.Queries;
using EntityDb.MongoDb.Commands;
using EntityDb.MongoDb.Envelopes;
using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Queries;
using EntityDb.MongoDb.Queries.FilterBuilders;
using EntityDb.MongoDb.Queries.SortBuilders;
using EntityDb.MongoDb.Sessions;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Documents;

internal sealed record CommandDocument : DocumentBase, IEntityDocument
{
    public const string CollectionName = "Commands";

    private static readonly CommandFilterBuilder FilterBuilder = new();

    private static readonly CommandSortBuilder SortBuilder = new();

    public Guid EntityId { get; init; }
    public ulong EntityVersionNumber { get; init; }
    
    public static InsertDocumentsCommand<CommandDocument> GetInsertCommand
    (
        IMongoSession mongoSession,
        ITransaction transaction,
        IAppendCommandTransactionStep appendCommandTransactionStep
    )
    {
        var documents = new[]
        {
            new CommandDocument
            {
                TransactionTimeStamp = transaction.TimeStamp,
                TransactionId = transaction.Id,
                EntityId = appendCommandTransactionStep.EntityId,
                EntityVersionNumber = appendCommandTransactionStep.EntityVersionNumber,
                Data = BsonDocumentEnvelope.Deconstruct(appendCommandTransactionStep.Command, mongoSession.Logger)
            }
        };
        
        return new InsertDocumentsCommand<CommandDocument>
        (
            mongoSession,
            CollectionName,
            documents
        );
    }

    public static DocumentQuery<CommandDocument> GetQuery
    (
        IMongoSession mongoSession,
        ICommandQuery commandQuery
    )
    {
        return new DocumentQuery<CommandDocument>
        (
            mongoSession,
            CollectionName,
            commandQuery.GetFilter(FilterBuilder),
            commandQuery.GetSort(SortBuilder),
            commandQuery.Skip,
            commandQuery.Take
        );
    }

    public static Task<ulong> GetLastEntityVersionNumber
    (
        IMongoSession mongoSession,
        Guid entityId
    )
    {
        var commandQuery = new GetLastEntityVersionQuery(entityId);

        var documentQuery = GetQuery
        (
            mongoSession,
            commandQuery
        );

        return documentQuery.GetEntityVersionNumber();
    }
}
