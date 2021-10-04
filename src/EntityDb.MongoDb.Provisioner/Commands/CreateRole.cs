using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Provisioner.MongoDbAtlas;
using EntityDb.MongoDb.Provisioner.MongoDbAtlas.Models;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Commands
{
    public class CreateRole : CommandBase
    {
        public static void AddTo(RootCommand rootCommand)
        {
            Command? createRole = new Command("create-role");

            AddMongoDbAtlasArgumentsTo(createRole);
            AddEntityNameArgumentTo(createRole);

            createRole.Handler = CommandHandler.Create(
                async (string groupName, string publicKey, string privateKey, string entityName) =>
                {
                    await Execute(groupName, publicKey, privateKey, entityName);
                });

            rootCommand.AddCommand(createRole);
        }

        public static async Task Execute(string groupName, string publicKey, string privateKey, string entityName)
        {
            using MongoDbAtlasClient? mongoDbAtlasClient =
                await GetMongoDbAtlasClient(groupName, publicKey, privateKey);

            if (await mongoDbAtlasClient.RoleExists(entityName))
            {
                return;
            }

            MongoDbAtlasResource? allClusterResources = new MongoDbAtlasResource { Cluster = true };

            MongoDbAtlasResource? allDbResources = new MongoDbAtlasResource { Db = entityName };

            MongoDbAtlasResource? sourceResource = new MongoDbAtlasResource
            {
                Db = entityName, Collection = SourceDocument.CollectionName
            };

            MongoDbAtlasResource? commandResource = new MongoDbAtlasResource
            {
                Db = entityName, Collection = CommandDocument.CollectionName
            };

            MongoDbAtlasResource? factResource = new MongoDbAtlasResource
            {
                Db = entityName, Collection = FactDocument.CollectionName
            };

            MongoDbAtlasResource? leaseResource = new MongoDbAtlasResource
            {
                Db = entityName, Collection = LeaseDocument.CollectionName
            };

            MongoDbAtlasRoleAction[]? roleActions = new[]
            {
                new MongoDbAtlasRoleAction { Action = "LIST_DATABASES", Resources = new[] { allClusterResources } },
                new MongoDbAtlasRoleAction { Action = "LIST_COLLECTIONS", Resources = new[] { allDbResources } },
                new MongoDbAtlasRoleAction
                {
                    Action = "FIND",
                    Resources = new[] { sourceResource, commandResource, factResource, leaseResource }
                },
                new MongoDbAtlasRoleAction
                {
                    Action = "INSERT",
                    Resources = new[] { sourceResource, commandResource, factResource, leaseResource }
                },
                new MongoDbAtlasRoleAction
                {
                    Action = "CREATE_INDEX",
                    Resources = new[] { sourceResource, commandResource, factResource, leaseResource }
                },
                new MongoDbAtlasRoleAction { Action = "REMOVE", Resources = new[] { leaseResource } }
            };

            await mongoDbAtlasClient.CreateRole(entityName, roleActions);
        }
    }
}
