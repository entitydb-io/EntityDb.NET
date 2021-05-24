using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace EntityDb.Redis.Sessions
{
    internal interface IRedisSession : IAsyncDisposable
    {
        Task<TResult> ExecuteQuery<TResult>(Func<IServiceProvider, IDatabase, Task<TResult>> query, TResult defaultResult);
        Task<bool> ExecuteCommand(Func<IServiceProvider, ITransaction, Task<bool>> command);
    }
}
