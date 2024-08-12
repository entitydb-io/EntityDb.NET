using EntityDb.Provisioner.MongoDbAtlas.Models;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace EntityDb.Provisioner.Commands.MongoDb.Atlas.Cluster;

internal sealed class CreateUser : CommandBase
{
    private static async Task Execute(Arguments arguments)
    {
        const string adminDatabaseName = "admin";

        using var mongoDbAtlasClient = await GetMongoDbAtlasClient
        (
            arguments.GroupName,
            arguments.PublicKey,
            arguments.PrivateKey
        );

        var userExists = await mongoDbAtlasClient.UserExists
        (
            adminDatabaseName,
            arguments.ServiceName
        );

        if (userExists)
        {
            return;
        }

        var roles = new[]
        {
            new MongoDbAtlasUserRole { DatabaseName = adminDatabaseName, RoleName = arguments.ServiceName },
        };

        await mongoDbAtlasClient.CreateUser
        (
            adminDatabaseName,
            arguments.ServicePassword,
            arguments.ServicePassword,
            roles
        );
    }

    public static void AddTo(Command parentCommand)
    {
        var createUser = new Command("create-user");

        AddMongoDbAtlasArguments(createUser);
        AddServiceNameArgument(createUser);
        AddServicePasswordArgument(createUser);

        createUser.Handler = CommandHandler.Create<Arguments>(Execute);

        parentCommand.AddCommand(createUser);
    }

    public sealed record Arguments
    (
        string GroupName,
        string PublicKey,
        string PrivateKey,
        string ServiceName,
        string ServicePassword
    );
}
