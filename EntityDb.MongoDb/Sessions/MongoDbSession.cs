using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Exceptions;
using MongoDB.Driver;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal class MongoDbSession : IMongoDbSession
    {
        protected readonly IClientSessionHandle? _clientSessionHandle;
        protected readonly IMongoDatabase _mongoDatabase;
        protected readonly ILogger _logger;
        protected readonly IResolvingStrategyChain _resolvingStrategyChain;

        public MongoDbSession
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ILogger logger,
            IResolvingStrategyChain resolvingStrategyChain
        )
        {
            _clientSessionHandle = clientSessionHandle;
            _mongoDatabase = mongoDatabase;
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

        public Task<TResult> ExecuteQuery<TResult>(Func<ILogger, IResolvingStrategyChain, IClientSessionHandle?, IMongoDatabase, Task<TResult>> query, TResult defaultResult)
        {
            return Execute
            (
                async () => await query.Invoke(_logger, _resolvingStrategyChain, _clientSessionHandle, _mongoDatabase),
                () =>
                {
                    return Task.FromResult(defaultResult);
                }
            );
        }

        [ExcludeFromCodeCoverage(Justification = "Tests use TestMode.")]
        public virtual async Task<bool> ExecuteCommand(Func<ILogger, IClientSessionHandle, IMongoDatabase, Task> command)
        {
            if (_clientSessionHandle == null)
            {
                throw new CannotWriteInReadOnlyModeException();
            }

            return await Execute(TryCommit, Abort);

            [ExcludeFromCodeCoverage]
            async Task<bool> TryCommit()
            {
                _clientSessionHandle.StartTransaction();

                await command.Invoke(_logger, _clientSessionHandle, _mongoDatabase);

                await _clientSessionHandle.CommitTransactionAsync();

                return true;
            }

            [ExcludeFromCodeCoverage]
            async Task<bool> Abort()
            {
                await _clientSessionHandle.AbortTransactionAsync();

                return false;
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

            if (_clientSessionHandle != null)
            {
                _clientSessionHandle.Dispose();
            }
        }
    }
}
