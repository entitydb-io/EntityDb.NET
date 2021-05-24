using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace EntityDb.Redis.Sessions
{
    internal class RedisSession : IRedisSession
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IConnectionMultiplexer _connectionMultiplexer;
        protected readonly ILogger _logger;

        public RedisSession
        (
            IServiceProvider serviceProvider,
            IConnectionMultiplexer connectionMultiplexer
        )
        {
            _serviceProvider = serviceProvider;
            _connectionMultiplexer = connectionMultiplexer;
            _logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(RedisSession));
        }

        protected async Task<TResult> Execute<TResult>(Func<Task<TResult>> tryOperation, Func<Task<TResult>> catchResult)
        {
            try
            {
                return await tryOperation.Invoke();
            }
            catch (Exception exception)
            {
                var eventId = new EventId(exception.GetHashCode(), exception.GetType().Name);

                _logger.LogError(eventId, exception, "The operation cannot be completed.");

                return await catchResult.Invoke();
            }
        }

        public Task<TResult> ExecuteQuery<TResult>(Func<IServiceProvider, IDatabase, Task<TResult>> query, TResult defaultResult)
        {
            var redisDatabase = _connectionMultiplexer.GetDatabase();

            return Execute
            (
                async () => await query.Invoke(_serviceProvider, redisDatabase),
                () => Task.FromResult(defaultResult)
            );
        }

        public virtual async Task<bool> ExecuteCommand(Func<IServiceProvider, ITransaction, Task<bool>> command)
        {
            var redisTransaction = _connectionMultiplexer.GetDatabase().CreateTransaction();

            return await Execute(TryCommit, Abort);

            async Task<bool> TryCommit()
            {
                var commandTask = command.Invoke(_serviceProvider, redisTransaction);

                await redisTransaction.ExecuteAsync();

                return await commandTask;
            }

            Task<bool> Abort()
            {
                return Task.FromResult(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Task.Yield();

            _connectionMultiplexer.Dispose();
        }
    }
}
