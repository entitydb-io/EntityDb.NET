using EntityDb.Provisioner.Commands.MongoDb.Atlas;
using System.CommandLine;

namespace EntityDb.Provisioner.Commands.MongoDb;

internal class MongoDbCommand
{
    public static void AddTo(Command parentCommand)
    {
        var mongoDb = new Command("mongodb");

        CreateCollections.AddTo(mongoDb);
        MongoDbAtlasCommand.AddTo(parentCommand);

        parentCommand.AddCommand(mongoDb);
    }
}
