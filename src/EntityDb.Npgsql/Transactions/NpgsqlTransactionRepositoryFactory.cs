using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Transactions;
using EntityDb.Npgsql.Queries;
using EntityDb.SqlDb.Sessions;
using EntityDb.SqlDb.Transactions;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Npgsql.Transactions;

internal class NpgsqlTransactionRepositoryFactory : DisposableResourceBaseClass, ISqlDbTransactionRepositoryFactory<NpgsqlQueryOptions>
{
    private readonly IEnvelopeService<string> _envelopeService;
    private readonly IOptionsFactory<SqlDbTransactionSessionOptions> _optionsFactory;
    private readonly IServiceProvider _serviceProvider;

    public NpgsqlTransactionRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IOptionsFactory<SqlDbTransactionSessionOptions> optionsFactory,
        IEnvelopeService<string> envelopeService
    )
    {
        _serviceProvider = serviceProvider;
        _optionsFactory = optionsFactory;
        _envelopeService = envelopeService;
    }

    public SqlDbTransactionSessionOptions GetTransactionSessionOptions(string transactionSessionOptionsName)
    {
        return _optionsFactory.Create(transactionSessionOptionsName);
    }

    public async Task<ISqlDbSession<NpgsqlQueryOptions>> CreateSession(SqlDbTransactionSessionOptions options,
        CancellationToken cancellationToken)
    {
        var npgsqlConnection = new NpgsqlConnection(options.ConnectionString);

        await npgsqlConnection.OpenAsync(cancellationToken);

        return SqlDbSession<NpgsqlQueryOptions>.Create
        (
            _serviceProvider,
            npgsqlConnection,
            options
        );
    }

    public ITransactionRepository CreateRepository
    (
        ISqlDbSession<NpgsqlQueryOptions> sqlDbSession
    )
    {
        var npgsqlTransactionRepository = new SqlDbTransactionRepository<NpgsqlQueryOptions>
        (
            sqlDbSession,
            _envelopeService
        );

        return TryCatchTransactionRepository.Create(_serviceProvider, npgsqlTransactionRepository);
    }
}
