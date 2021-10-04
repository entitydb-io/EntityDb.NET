using EntityDb.MongoDb.Provisioner.Extensions;
using MongoDB.Driver;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Commands
{
    public class CreateCollections : CommandBase
    {
        public static void AddTo(RootCommand rootCommand)
        {
            var createCollections = new Command("create-collections");

            AddMongoDbAtlasArgumentsTo(createCollections);
            AddClusterNameArgumentTo(createCollections);
            AddEntityNameArgumentTo(createCollections);
            AddEntityPasswordArgumentTo(createCollections);

            createCollections.Handler = CommandHandler.Create(
                async (string groupName, string publicKey, string privateKey, string clusterName, string entityName,
                    string entityPassword) =>
                {
                    await Execute(groupName, publicKey, privateKey, clusterName, entityName, entityPassword);
                });

            rootCommand.AddCommand(createCollections);
        }

        public static async Task Execute(string groupName, string publicKey, string privateKey, string clusterName,
            string entityName, string entityPassword)
        {
            const string protocol = "mongodb+srv://";

            var mongoDbAtlasClient = await GetMongoDbAtlasClient(groupName, publicKey, privateKey);

            var cluster = await mongoDbAtlasClient.GetCluster(clusterName);

            if (cluster == null || cluster.SrvAddress?.StartsWith(protocol) != true)
            {
                throw new InvalidOperationException();
            }

            var mongoClient =
                new MongoClient(
                    $"{protocol}{entityName}:{entityPassword}@{cluster.SrvAddress[protocol.Length..]}/admin");

            await mongoClient.ProvisionCollections(entityName);
        }
    }
}
