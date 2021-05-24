using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Sessions
{
    internal sealed class TestModeMongoDbSession : MongoDbSession
    {
        public TestModeMongoDbSession
        (
            IServiceProvider serviceProvider,
            IClientSessionHandle? clientSessionHandle,
            IMongoDatabase mongoDatabase
        ) : base
        (
            serviceProvider,
            clientSessionHandle,
            mongoDatabase
        )
        {
            if (clientSessionHandle != null)
            {
                clientSessionHandle.StartTransaction();
            }
        }

        public override async Task<bool> ExecuteCommand(Func<IServiceProvider, IClientSessionHandle, IMongoDatabase, Task> command)
        {
            if (_clientSessionHandle == null)
            {
                return false;
            }

            return await Execute
            (
                async () =>
                {
                    await command.Invoke(_serviceProvider, _clientSessionHandle, _mongoDatabase);

                    return true;
                },
                () => Task.FromResult(false)
            );
        }
    }
}
