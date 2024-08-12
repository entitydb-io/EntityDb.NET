using EntityDb.MongoDb.Extensions;
using MongoDB.Driver;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace EntityDb.Provisioner.Commands.MongoDb;

internal sealed class CreateCollections : CommandBase
{
    private static async Task Execute(Arguments arguments)
    {
        var mongoClient = new MongoClient(arguments.ConnectionString);

        await mongoClient.ProvisionSourceCollections(arguments.ServiceName);
    }

    private static void AddConnectionStringArgument(Command command)
    {
        var connectionStringArgument = new Argument<string>("connection-string")
        {
            Description = "The connection string to the mongodb instance.",
        };

        command.AddArgument(connectionStringArgument);
    }

    public static void AddTo(Command parentCommand)
    {
        var createCollectionsDirect = new Command("create-collections")
        {
            Handler = CommandHandler.Create<Arguments>(Execute),
        };

        AddServiceNameArgument(createCollectionsDirect);
        AddConnectionStringArgument(createCollectionsDirect);

        parentCommand.AddCommand(createCollectionsDirect);
    }

    public sealed record Arguments
    (
        string ServiceName,
        string ConnectionString
    );
}
