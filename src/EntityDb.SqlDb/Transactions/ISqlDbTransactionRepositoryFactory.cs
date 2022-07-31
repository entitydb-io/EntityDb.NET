using EntityDb.Abstractions.Transactions;
using EntityDb.SqlDb.Sessions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Transactions;

internal interface ISqlDbTransactionRepositoryFactory<TOptions> : ITransactionRepositoryFactory
    where TOptions : class
{
    async Task<ITransactionRepository> ITransactionRepositoryFactory.CreateRepository(
        string transactionSessionOptionsName, CancellationToken cancellationToken)
    {
        var options = GetTransactionSessionOptions(transactionSessionOptionsName);

        var sqlDbSession = await CreateSession(options, cancellationToken);

        return CreateRepository(sqlDbSession);
    }

    SqlDbTransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName);

    Task<ISqlDbSession<TOptions>> CreateSession(SqlDbTransactionSessionOptions options,
        CancellationToken cancellationToken);

    ITransactionRepository CreateRepository(ISqlDbSession<TOptions> sqlDbSession);
}
