using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Queries;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal interface IMongoDbSession : IDisposable, IAsyncDisposable
    {
        Task<TOut[]> ExecuteDocumentQuery<TDocument, TOut>(
            Func<IClientSessionHandle?, IMongoDatabase, DocumentQuery<TDocument>> queryBuilder,
            Func<TDocument, ILogger, IResolvingStrategyChain, TOut> converter)
            where TDocument : ITransactionDocument;
        
        Task<TData[]> ExecuteDataQuery<TDocument, TData>(
            Func<IClientSessionHandle?, IMongoDatabase, DataQuery<TDocument>> queryBuilder)
            where TDocument : ITransactionDocument;

        Task<Guid[]> ExecuteGuidQuery<TDocument>(
            Func<IClientSessionHandle?, IMongoDatabase, GuidQuery<TDocument>> queryBuilder)
            where TDocument : ITransactionDocument;

        Task<bool> ExecuteCommand(Func<ILogger, IClientSessionHandle, IMongoDatabase, Task> command);
    }
}
