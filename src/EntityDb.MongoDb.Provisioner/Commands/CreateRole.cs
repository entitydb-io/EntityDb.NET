using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Provisioner.MongoDbAtlas.Models;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Commands;

internal class CreateRole : CommandBase
{
    public static void AddTo(RootCommand rootCommand)
    {
        var createRole = new Command("create-role");

        AddMongoDbAtlasArgumentsTo(createRole);
        AddEntityNameArgumentTo(createRole);

        createRole.Handler = CommandHandler.Create(
            async (string groupName, string publicKey, string privateKey, string entityName) =>
            {
                await Execute(groupName, publicKey, privateKey, entityName);
            });

        rootCommand.AddCommand(createRole);
    }

    private static async Task Execute(string groupName, string publicKey, string privateKey, string entityName)
    {
        using var mongoDbAtlasClient = await GetMongoDbAtlasClient(groupName, publicKey, privateKey);

        if (await mongoDbAtlasClient.RoleExists(entityName))
        {
            return;
        }

        var allClusterResources = new MongoDbAtlasResource { Cluster = true };

        var allDbResources = new MongoDbAtlasResource { Db = entityName };

        var agentSignatureResource = new MongoDbAtlasResource
        {
            Db = entityName,
            Collection = AgentSignatureDocument.CollectionName
        };

        var commandResource = new MongoDbAtlasResource { Db = entityName, Collection = CommandDocument.CollectionName };

        var leaseResource = new MongoDbAtlasResource { Db = entityName, Collection = LeaseDocument.CollectionName };

        var tagResources = new MongoDbAtlasResource { Db = entityName, Collection = TagDocument.CollectionName };

        var roleActions = new[]
        {
            new MongoDbAtlasRoleAction { Action = "LIST_DATABASES", Resources = new[] { allClusterResources } },
            new MongoDbAtlasRoleAction { Action = "LIST_COLLECTIONS", Resources = new[] { allDbResources } },
            new MongoDbAtlasRoleAction
            {
                Action = "FIND",
                Resources = new[] { agentSignatureResource, commandResource, leaseResource, tagResources }
            },
            new MongoDbAtlasRoleAction
            {
                Action = "INSERT",
                Resources = new[] { agentSignatureResource, commandResource, leaseResource, tagResources }
            },
            new MongoDbAtlasRoleAction
            {
                Action = "CREATE_INDEX",
                Resources = new[] { agentSignatureResource, commandResource, leaseResource, tagResources }
            },
            new MongoDbAtlasRoleAction { Action = "REMOVE", Resources = new[] { leaseResource, tagResources } }
        };

        await mongoDbAtlasClient.CreateRole(entityName, roleActions);
    }
}
