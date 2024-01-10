using EntityDb.MongoDb.Extensions;
using MongoDB.Driver;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace EntityDb.Provisioner.Commands.MongoDb.Atlas.Cluster;

internal class CreateCollections : CommandBase
{
    private static async Task Execute(Arguments arguments)
    {
        const string expectedProtocol = "mongodb+srv://";

        var mongoDbAtlasClient = await GetMongoDbAtlasClient
        (
            arguments.GroupName,
            arguments.PublicKey,
            arguments.PrivateKey
        );

        var cluster = await mongoDbAtlasClient.GetCluster(arguments.ClusterName);

        if (cluster?.SrvAddress?.StartsWith(expectedProtocol) != true)
        {
            throw new InvalidOperationException();
        }

        var mongoClient =
            new MongoClient(
                $"{expectedProtocol}{arguments.ServiceName}:{arguments.ServicePassword}@{cluster.SrvAddress[expectedProtocol.Length..]}/admin");

        await mongoClient.ProvisionSourceCollections(arguments.ServiceName);
    }

    protected static void AddClusterNameArgument(Command command)
    {
        var clusterNameArgument = new Argument<string>("cluster-name")
        {
            Description = "The name of the Cluster on which the collections will be provisioned.",
        };

        command.AddArgument(clusterNameArgument);
    }

    public static void AddTo(Command parentCommand)
    {
        var createCollections = new Command("create-collections")
        {
            Handler = CommandHandler.Create<Arguments>(Execute),
        };

        AddMongoDbAtlasArguments(createCollections);
        AddServiceNameArgument(createCollections);
        AddServicePasswordArgument(createCollections);
        AddClusterNameArgument(createCollections);

        parentCommand.AddCommand(createCollections);
    }

    public record Arguments
    (
        string GroupName,
        string PublicKey,
        string PrivateKey,
        string ServiceName,
        string ServicePassword,
        string ClusterName
    );
}
