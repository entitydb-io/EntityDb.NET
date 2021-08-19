using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal interface IMongoDbSession : IDisposable, IAsyncDisposable
    {
        Task<TResult> ExecuteQuery<TResult>(Func<ILogger, IResolvingStrategyChain, IClientSessionHandle?, IMongoDatabase, Task<TResult>> query, TResult defaultResult);
        Task<bool> ExecuteCommand(Func<ILogger, IClientSessionHandle, IMongoDatabase, Task> command);
    }
}
