using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal class MongoDbSession : IMongoDbSession
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly IClientSessionHandle? _clientSessionHandle;
        protected readonly IMongoDatabase _mongoDatabase;
        protected readonly ILogger _logger;

        public MongoDbSession
        (
            IServiceProvider serviceProvider,
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase
        )
        {
            _serviceProvider = serviceProvider;
            _clientSessionHandle = clientSessionHandle;
            _mongoDatabase = mongoDatabase;
            _logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(MongoDbSession));
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

        public Task<TResult> ExecuteQuery<TResult>(Func<IServiceProvider, IClientSessionHandle?, IMongoDatabase, Task<TResult>> query, TResult defaultResult)
        {
            return Execute
            (
                async () => await query.Invoke(_serviceProvider, _clientSessionHandle, _mongoDatabase),
                () =>
                {
                    return Task.FromResult(defaultResult);
                }
            );
        }

        [ExcludeFromCodeCoverage(Justification = "Tests use TestMode.")]
        public virtual async Task<bool> ExecuteCommand(Func<IServiceProvider, IClientSessionHandle, IMongoDatabase, Task> command)
        {
            if (_clientSessionHandle == null)
            {
                return false;
            }

            return await Execute(TryCommit, Abort);

            [ExcludeFromCodeCoverage]
            async Task<bool> TryCommit()
            {
                _clientSessionHandle.StartTransaction();

                await command.Invoke(_serviceProvider, _clientSessionHandle, _mongoDatabase);

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
