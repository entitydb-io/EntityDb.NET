using EntityDb.MongoDb.Documents;
using EntityDb.Provisioner.MongoDbAtlas.Models;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace EntityDb.Provisioner.Commands.MongoDb.Atlas.Cluster;

internal sealed class CreateRole : CommandBase
{
    private static async Task Execute(Arguments arguments)
    {
        using var mongoDbAtlasClient = await GetMongoDbAtlasClient
        (
            arguments.GroupName,
            arguments.PublicKey,
            arguments.PrivateKey
        );

        if (await mongoDbAtlasClient.RoleExists(arguments.ServiceName))
        {
            return;
        }

        var allClusterResources = new MongoDbAtlasResource { Cluster = true };

        var allDbResources = new MongoDbAtlasResource { Db = arguments.ServiceName };

        var agentSignatureResource = new MongoDbAtlasResource
        {
            Db = arguments.ServiceName, Collection = AgentSignatureDocument.CollectionName,
        };

        var commandResource = new MongoDbAtlasResource
        {
            Db = arguments.ServiceName, Collection = DeltaDataDocument.CollectionName,
        };

        var leaseResource = new MongoDbAtlasResource
        {
            Db = arguments.ServiceName, Collection = LeaseDataDocument.CollectionName,
        };

        var tagResources = new MongoDbAtlasResource
        {
            Db = arguments.ServiceName, Collection = TagDataDocument.CollectionName,
        };

        var roleActions = new[]
        {
            new MongoDbAtlasRoleAction { Action = "LIST_DATABASES", Resources = new[] { allClusterResources } },
            new MongoDbAtlasRoleAction { Action = "LIST_COLLECTIONS", Resources = new[] { allDbResources } },
            new MongoDbAtlasRoleAction
            {
                Action = "FIND",
                Resources = new[] { agentSignatureResource, commandResource, leaseResource, tagResources },
            },
            new MongoDbAtlasRoleAction
            {
                Action = "INSERT",
                Resources = new[] { agentSignatureResource, commandResource, leaseResource, tagResources },
            },
            new MongoDbAtlasRoleAction
            {
                Action = "CREATE_INDEX",
                Resources = new[] { agentSignatureResource, commandResource, leaseResource, tagResources },
            },
            new MongoDbAtlasRoleAction { Action = "REMOVE", Resources = new[] { leaseResource, tagResources } },
        };

        await mongoDbAtlasClient.CreateRole
        (
            arguments.ServiceName,
            roleActions
        );
    }

    public static void AddTo(Command parentCommand)
    {
        var createRole = new Command("create-role") { Handler = CommandHandler.Create<Arguments>(Execute) };

        AddMongoDbAtlasArguments(createRole);
        AddServiceNameArgument(createRole);

        parentCommand.AddCommand(createRole);
    }

    public sealed record Arguments
    (
        string GroupName,
        string PublicKey,
        string PrivateKey,
        string ServiceName
    );
}
