using EntityDb.Abstractions.Transactions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions.Processors;

internal interface ITransactionProcessor
{
    Task ProcessTransaction(ITransaction transaction, CancellationToken cancellationToken);
}
