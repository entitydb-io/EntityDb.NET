using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using StackExchange.Redis;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.Redis.Sessions
{
    internal class RedisSession : IRedisSession
    {
        protected readonly IConnectionMultiplexer _connectionMultiplexer;
        protected readonly ILogger _logger;
        protected readonly IResolvingStrategyChain _resolvingStrategyChain;

        public RedisSession
        (
            IConnectionMultiplexer connectionMultiplexer,
            ILogger logger,
            IResolvingStrategyChain resolvingStrategyChain
        )
        {
            _connectionMultiplexer = connectionMultiplexer;
            _logger = logger;
            _resolvingStrategyChain = resolvingStrategyChain;
        }

        protected async Task<TResult> Execute<TResult>(Func<Task<TResult>> tryOperation, Func<Task<TResult>> catchResult)
        {
            try
            {
                return await tryOperation.Invoke();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "The operation cannot be completed.");

                return await catchResult.Invoke();
            }
        }

        public Task<TResult> ExecuteQuery<TResult>(Func<ILogger, IResolvingStrategyChain, IDatabase, Task<TResult>> query, TResult defaultResult)
        {
            var redisDatabase = _connectionMultiplexer.GetDatabase();

            return Execute
            (
                async () => await query.Invoke(_logger, _resolvingStrategyChain, redisDatabase),
                () => Task.FromResult(defaultResult)
            );
        }

        public virtual async Task<bool> ExecuteCommand(Func<ILogger, ITransaction, Task<bool>> command)
        {
            var redisTransaction = _connectionMultiplexer.GetDatabase().CreateTransaction();

            return await Execute(TryCommit, Abort);

            async Task<bool> TryCommit()
            {
                var commandTask = command.Invoke(_logger, redisTransaction);

                await redisTransaction.ExecuteAsync();

                return await commandTask;
            }

            Task<bool> Abort()
            {
                return Task.FromResult(false);
            }
        }

        [ExcludeFromCodeCoverage]
        public void Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        public async ValueTask DisposeAsync()
        {
            await Task.Yield();

            _connectionMultiplexer.Dispose();
        }
    }
}
