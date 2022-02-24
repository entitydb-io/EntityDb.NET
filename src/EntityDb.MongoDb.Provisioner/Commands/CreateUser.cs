using EntityDb.MongoDb.Provisioner.MongoDbAtlas.Models;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Commands;

internal class CreateUser : CommandBase
{
    public static void AddTo(RootCommand rootCommand)
    {
        var createUser = new Command("create-user");

        AddMongoDbAtlasArgumentsTo(createUser);
        AddEntityNameArgumentTo(createUser);
        AddEntityPasswordArgumentTo(createUser);

        createUser.Handler = CommandHandler.Create(
            async (string groupName, string publicKey, string privateKey, string entityName,
                string entityPassword) =>
            {
                await Execute(groupName, publicKey, privateKey, entityName, entityPassword);
            });

        rootCommand.AddCommand(createUser);
    }

    private static async Task Execute(string groupName, string publicKey, string privateKey, string entityName,
        string entityPassword)
    {
        using var mongoDbAtlasClient = await GetMongoDbAtlasClient(groupName, publicKey, privateKey);

        if (await mongoDbAtlasClient.UserExists("admin", entityName))
        {
            return;
        }

        var roles = new[] { new MongoDbAtlasUserRole { DatabaseName = "admin", RoleName = entityName } };

        await mongoDbAtlasClient.CreateUser("admin", entityName, entityPassword, roles);
    }
}
