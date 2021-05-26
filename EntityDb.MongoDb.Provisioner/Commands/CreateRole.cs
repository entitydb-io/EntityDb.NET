using EntityDb.MongoDb.Documents;
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
            var createRole = new Command("create-role");

            AddMongoDbAtlasArgumentsTo(createRole);
            AddEntityNameArgumentTo(createRole);

            createRole.Handler = CommandHandler.Create(async (string groupName, string publicKey, string privateKey, string entityName) =>
            {
                await Execute(groupName, publicKey, privateKey, entityName);
            });

            rootCommand.AddCommand(createRole);
        }

        public static async Task Execute(string groupName, string publicKey, string privateKey, string entityName)
        {
            using var mongoDbAtlasClient = await GetMongoDbAtlasClient(groupName, publicKey, privateKey);

            if (await mongoDbAtlasClient.RoleExists(entityName))
            {
                return;
            }

            var allClusterResources = new MongoDbAtlasResource
            {
                Cluster = true,
            };

            var allDbResources = new MongoDbAtlasResource
            {
                Db = entityName,
            };

            var sourceResource = new MongoDbAtlasResource
            {
                Db = entityName,
                Collection = SourceDocument.CollectionName,
            };

            var commandResource = new MongoDbAtlasResource
            {
                Db = entityName,
                Collection = CommandDocument.CollectionName,
            };

            var factResource = new MongoDbAtlasResource
            {
                Db = entityName,
                Collection = FactDocument.CollectionName,
            };

            var tagResource = new MongoDbAtlasResource
            {
                Db = entityName,
                Collection = TagDocument.CollectionName,
            };

            var roleActions = new[]
            {
                new MongoDbAtlasRoleAction
                {
                    Action = "LIST_DATABASES",
                    Resources = new[]
                    {
                        allClusterResources,
                    },
                },
                new MongoDbAtlasRoleAction
                {
                    Action = "LIST_COLLECTIONS",
                    Resources = new[]
                    {
                        allDbResources,
                    },
                },
                new MongoDbAtlasRoleAction
                {
                    Action = "FIND",
                    Resources = new[]
                    {
                        sourceResource,
                        commandResource,
                        factResource,
                        tagResource,
                    },
                },
                new MongoDbAtlasRoleAction
                {
                    Action = "INSERT",
                    Resources = new[]
                    {
                        sourceResource,
                        commandResource,
                        factResource,
                        tagResource,
                    },
                },
                new MongoDbAtlasRoleAction
                {
                    Action = "CREATE_INDEX",
                    Resources = new[]
                    {
                        sourceResource,
                        commandResource,
                        factResource,
                        tagResource,
                    },
                },
                new MongoDbAtlasRoleAction
                {
                    Action = "REMOVE",
                    Resources = new[]
                    {
                        tagResource,
                    },
                },
            };

            await mongoDbAtlasClient.CreateRole(entityName, roleActions);
        }
    }
}
