using EntityDb.Abstractions.Loggers;
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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Documents
{
    internal sealed record CommandDocument : DocumentBase, IEntityDocument
    {
        public const string CollectionName = "Commands";

        private static readonly CommandFilterBuilder _filterBuilder = new();

        private static readonly CommandSortBuilder _sortBuilder = new();

        public Guid EntityId { get; init; }
        public ulong EntityVersionNumber { get; init; }

        public static IReadOnlyCollection<CommandDocument>? BuildInsert<TEntity>
        (
            ITransaction<TEntity> transaction,
            ICommandTransactionStep<TEntity> commandTransactionStep,
            ILogger logger
        )
        {
            return new[]
            {
                new CommandDocument
                {
                    TransactionTimeStamp = transaction.TimeStamp,
                    TransactionId = transaction.Id,
                    EntityId = commandTransactionStep.EntityId,
                    EntityVersionNumber = commandTransactionStep.NextEntityVersionNumber,
                    Data = BsonDocumentEnvelope.Deconstruct(commandTransactionStep.Command, logger)
                }
            };
        }

        public static InsertDocumentsCommand<TEntity, ICommandTransactionStep<TEntity>, CommandDocument> GetInsertCommand<TEntity>
        (
            IMongoSession mongoSession
        )
        {
            return new InsertDocumentsCommand<TEntity, ICommandTransactionStep<TEntity>, CommandDocument>
            (
                mongoSession,
                CollectionName,
                BuildInsert<TEntity>
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
                commandQuery.GetFilter(_filterBuilder),
                commandQuery.GetSort(_sortBuilder),
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
}
