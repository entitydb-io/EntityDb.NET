using EntityDb.Common.Disposables;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Redis.ConnectionMultiplexers;

internal class ConnectionMultiplexerFactory : DisposableResourceBaseClass
{
    private readonly ConcurrentDictionary<string, IConnectionMultiplexer> _connectionMultiplexers = new();
    private readonly SemaphoreSlim _connectionSemaphore = new(1);

    public async Task<IConnectionMultiplexer> CreateConnectionMultiplexer(string connectionString,
        CancellationToken cancellationToken)
    {
        await _connectionSemaphore.WaitAsync(cancellationToken);

        var connectionMultiplexer = _connectionMultiplexers.GetOrAdd(connectionString, _ =>
        {
            var configurationOptions = ConfigurationOptions.Parse(connectionString);

            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        _connectionSemaphore.Release();

        return connectionMultiplexer;
    }

    public override async ValueTask DisposeAsync()
    {
        await _connectionSemaphore.WaitAsync();

        foreach (var connectionMultiplexer in _connectionMultiplexers.Values)
        {
            connectionMultiplexer.Dispose();
        }

        _connectionSemaphore.Release();
    }
}
