using EntityDb.MongoDb.Provisioner.Extensions;
using MongoDB.Driver;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Commands;

internal class CreateCollectionsDirect : CommandBase
{
    public static void AddTo(RootCommand rootCommand)
    {
        var createCollectionsDirect = new Command("create-collections-direct");

        AddEntityNameArgumentTo(createCollectionsDirect);

        var connectionStringArgument =
            new Argument<string>("connection-string", "The connection string to the mongodb instance.");

        createCollectionsDirect.AddArgument(connectionStringArgument);

        createCollectionsDirect.Handler = CommandHandler.Create(
            async (string entityName, string connectionString) =>
            {
                await Execute(entityName, connectionString);
            });

        rootCommand.AddCommand(createCollectionsDirect);
    }

    private static async Task Execute(string entityName, string connectionString)
    {
        var mongoClient = new MongoClient(connectionString);

        await mongoClient.ProvisionCollections(entityName);
    }
}
