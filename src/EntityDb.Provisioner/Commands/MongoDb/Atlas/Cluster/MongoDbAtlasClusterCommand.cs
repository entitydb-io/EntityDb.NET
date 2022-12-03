using System.CommandLine;

namespace EntityDb.Provisioner.Commands.MongoDb.Atlas.Cluster;

internal class MongoDbAtlasClusterCommand
{
    public static void AddTo(Command parentCommand)
    {
        var cluster = new Command("cluster");

        CreateRole.AddTo(cluster);
        CreateUser.AddTo(cluster);
        CreateCollections.AddTo(cluster);

        parentCommand.AddCommand(cluster);
    }
}
