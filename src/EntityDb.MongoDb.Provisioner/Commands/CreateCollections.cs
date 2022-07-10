using EntityDb.MongoDb.Provisioner.Extensions;
using MongoDB.Driver;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Commands;

internal class CreateCollections : CommandBase
{
    public record Arguments
    (
        string GroupName,
        string PublicKey,
        string PrivateKey,
        string EntityName,
        string EntityPassword,
        string ClusterName
    );

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

        if (cluster is null || cluster.SrvAddress?.StartsWith(expectedProtocol) != true)
        {
            throw new InvalidOperationException();
        }

        var mongoClient =
            new MongoClient(
                $"{expectedProtocol}{arguments.EntityName}:{arguments.EntityPassword}@{cluster.SrvAddress[expectedProtocol.Length..]}/admin");

        await mongoClient.ProvisionCollections(arguments.EntityName);
    }

    public static void AddTo(RootCommand rootCommand)
    {
        var createCollections = new Command("create-collections")
        {
            Handler = CommandHandler.Create<Arguments>(Execute)
        };

        AddMongoDbAtlasArguments(createCollections);
        AddClusterNameArgument(createCollections);
        AddServiceNameArgument(createCollections);
        AddServicePasswordArgument(createCollections);

        rootCommand.AddCommand(createCollections);
    }
}
