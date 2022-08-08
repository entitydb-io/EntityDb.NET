using EntityDb.Provisioner.Commands.MongoDb.Atlas.Cluster;
using EntityDb.Provisioner.Commands.MongoDb.Atlas.Serverless;
using System.CommandLine;

namespace EntityDb.Provisioner.Commands.MongoDb.Atlas;

internal class MongoDbAtlasCommand
{
    public static void AddTo(Command parentCommand)
    {
        var atlas = new Command("atlas");

        MongoDbAtlasClusterCommand.AddTo(atlas);
        MongoDbAtlasServerlessCommand.AddTo(atlas);

        parentCommand.AddCommand(atlas);
    }
}
