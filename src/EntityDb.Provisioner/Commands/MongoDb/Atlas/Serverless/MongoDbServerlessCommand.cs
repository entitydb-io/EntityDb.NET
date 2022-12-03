using System.CommandLine;

namespace EntityDb.Provisioner.Commands.MongoDb.Atlas.Serverless;

internal class MongoDbAtlasServerlessCommand
{
    public static void AddTo(Command parentCommand)
    {
        var serverless = new Command("serverless");

        CreateCollections.AddTo(serverless);

        parentCommand.AddCommand(serverless);
    }
}
