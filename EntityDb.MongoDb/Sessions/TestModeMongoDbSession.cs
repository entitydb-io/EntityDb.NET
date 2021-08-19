using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Strategies;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal sealed class TestModeMongoDbSession : MongoDbSession
    {
        public TestModeMongoDbSession
        (
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase,
            ILogger logger,
            IResolvingStrategyChain resolvingStrategyChain
        ) : base
        (
            clientSessionHandle,
            mongoDatabase,
            logger,
            resolvingStrategyChain
        )
        {
            if (clientSessionHandle != null)
            {
                clientSessionHandle.StartTransaction();
            }
        }

        public override async Task<bool> ExecuteCommand(Func<ILogger, IClientSessionHandle, IMongoDatabase, Task> command)
        {
            if (_clientSessionHandle == null)
            {
                return false;
            }

            return await Execute
            (
                async () =>
                {
                    await command.Invoke(_logger, _clientSessionHandle, _mongoDatabase);

                    return true;
                },
                () => Task.FromResult(false)
            );
        }
    }
}
