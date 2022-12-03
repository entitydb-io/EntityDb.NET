using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Disposables;
using EntityDb.SqlDb.Sessions;

namespace EntityDb.SqlDb.Transactions;

internal abstract class SqlDbTransactionRepositoryFactoryWrapper<TOptions> : DisposableResourceBaseClass,
    ISqlDbTransactionRepositoryFactory<TOptions>
    where TOptions : class
{
    private readonly ISqlDbTransactionRepositoryFactory<TOptions> _sqlDbTransactionRepositoryFactory;

    protected SqlDbTransactionRepositoryFactoryWrapper(
        ISqlDbTransactionRepositoryFactory<TOptions> sqlDbTransactionRepositoryFactory)
    {
        _sqlDbTransactionRepositoryFactory = sqlDbTransactionRepositoryFactory;
    }

    public virtual SqlDbTransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName)
    {
        return _sqlDbTransactionRepositoryFactory.GetTransactionSessionOptions(transactionSessionOptionsName);
    }

    public virtual Task<ISqlDbSession<TOptions>> CreateSession(SqlDbTransactionSessionOptions options,
        CancellationToken cancellationToken)
    {
        return _sqlDbTransactionRepositoryFactory.CreateSession(options, cancellationToken);
    }

    public virtual ITransactionRepository CreateRepository
    (
        ISqlDbSession<TOptions> sqlDbSession
    )
    {
        return _sqlDbTransactionRepositoryFactory.CreateRepository(sqlDbSession);
    }

    public override async ValueTask DisposeAsync()
    {
        await _sqlDbTransactionRepositoryFactory.DisposeAsync();
    }
}
