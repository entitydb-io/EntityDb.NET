using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal interface IMongoDbSession : IDisposable, IAsyncDisposable
    {
        Task<TResult> ExecuteQuery<TResult>(Func<IServiceProvider, IClientSessionHandle?, IMongoDatabase, Task<TResult>> query, TResult defaultResult);
        Task<bool> ExecuteCommand(Func<IServiceProvider, IClientSessionHandle, IMongoDatabase, Task> command);
    }
}
