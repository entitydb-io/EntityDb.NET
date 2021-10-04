using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace EntityDb.Redis.Sessions
{
    internal interface IRedisSession : IDisposable, IAsyncDisposable
    {
        Task<TResult> ExecuteQuery<TResult>(Func<ILogger, IResolvingStrategyChain, IDatabase, Task<TResult>> query,
            TResult defaultResult);

        Task<bool> ExecuteCommand(Func<ILogger, ITransaction, Task<bool>> command);
    }
}
