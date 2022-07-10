using EntityDb.MongoDb.Provisioner.Extensions;
using MongoDB.Driver;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Commands;

internal class CreateCollectionsDirect : CommandBase
{
    public record Arguments
    (
        string ServiceName,
        string ConnectionString
    );

    private static async Task Execute(Arguments arguments)
    {
        var mongoClient = new MongoClient(arguments.ConnectionString);

        await mongoClient.ProvisionCollections(arguments.ServiceName);
    }

    private static void AddConnectionStringArgument(Command command)
    {
        var connectionStringArgument = new Argument<string>("connection-string")
        {
            Description = "The connection string to the mongodb instance."
        };

        command.AddArgument(connectionStringArgument);
    }

    public static void AddTo(RootCommand rootCommand)
    {
        var createCollectionsDirect = new Command("create-collections-direct")
        {
            Handler = CommandHandler.Create<Arguments>(Execute)
        };

        AddServiceNameArgument(createCollectionsDirect);
        AddConnectionStringArgument(createCollectionsDirect);

        rootCommand.AddCommand(createCollectionsDirect);
    }
}
