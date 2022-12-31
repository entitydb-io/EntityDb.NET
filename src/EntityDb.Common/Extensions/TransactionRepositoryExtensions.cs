using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Queries;
using EntityDb.Common.Transactions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EntityDb.Common.Extensions
{
    internal static class TransactionRepositoryExtensions
    {
        public static async IAsyncEnumerable<ITransaction> EnumerateTransactions(this ITransactionRepository transactionRepository, ICommandQuery commandQuery, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var allAnnotatedCommands = await transactionRepository
                .EnumerateAnnotatedCommands(commandQuery, cancellationToken)
                .ToLookupAsync(annotatedCommand => annotatedCommand.TransactionId, cancellationToken);

            var transactionIds = allAnnotatedCommands
                .SelectMany(annotatedCommands => annotatedCommands
                    .Select(annotatedCommand => annotatedCommand.TransactionId))
                .Distinct()
                .ToArray();

            var agentSignatureQuery = new GetAgentSignatures(transactionIds);

            var annotatedAgentSignatures = transactionRepository
                .EnumerateAnnotatedAgentSignatures(agentSignatureQuery, cancellationToken);

            await foreach (var annotatedAgentSignature in annotatedAgentSignatures)
            {
                var annotatedCommands = allAnnotatedCommands[annotatedAgentSignature.TransactionId];

                yield return new Transaction
                {
                    Id = annotatedAgentSignature.TransactionId,
                    TimeStamp = annotatedAgentSignature.TransactionTimeStamp,
                    AgentSignature = annotatedAgentSignature.Data,
                    Commands = annotatedCommands
                        .Select(annotatedCommand => new TransactionCommand
                        {
                            EntityId = annotatedCommand.EntityId,
                            EntityVersionNumber = annotatedCommand.EntityVersionNumber,
                            Command = annotatedCommand.Data
                        })
                        .ToImmutableArray<ITransactionCommand>()
                };
            }
        }
    }
}
