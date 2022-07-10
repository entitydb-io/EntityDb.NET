using EntityDb.MongoDb.Provisioner.MongoDbAtlas;
using System.CommandLine;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Commands;

internal abstract class CommandBase
{
    private static readonly Regex ServiceNameRegex = new("^[a-z][a-z]*$", RegexOptions.IgnoreCase);

    protected static void AddMongoDbAtlasArguments(Command command)
    {
        var groupName = new Argument<string>("group-name")
        {
            Description = "The name of the MongoDb Atlas Group/Project to access via API."
        };

        var publicKey = new Argument<string>("public-key")
        {
            Description = "The public key used to authenticate via API."
        };

        var privateKey = new Argument<string>("private-key")
        {
            Description = "The private key used to authenticate via API."
        };

        command.AddArgument(groupName);
        command.AddArgument(publicKey);
        command.AddArgument(privateKey);
    }

    protected static void AddClusterNameArgument(Command command)
    {
        var clusterNameArgument = new Argument<string>("cluster-name")
        {
            Description = "The name of the Cluster on which the entity will be provisioned."
        };

        command.AddArgument(clusterNameArgument);
    }

    protected static void AddServiceNameArgument(Command command)
    {
        var serviceName = new Argument<string>("service-name")
        {
            Description = "The name of the service that will use this database."
        };

        serviceName.AddValidator(serviceNameResult =>
        {
            var serviceName = serviceNameResult.GetValueOrDefault<string>() ?? string.Empty;

            if (!ServiceNameRegex.IsMatch(serviceName))
            {
                serviceNameResult.ErrorMessage = "The service name must begin with an letter, and can only contain letters.";
            }
        });

        command.AddArgument(serviceName);
    }

    protected static void AddServicePasswordArgument(Command command)
    {
        var servicePassword = new Argument<string>("service-password")
        {
            Description = "The password for the service that will use this database."
        };

        command.AddArgument(servicePassword);
    }

    internal static Task<MongoDbAtlasClient> GetMongoDbAtlasClient(string groupName, string publicKey,
        string privateKey)
    {
        return MongoDbAtlasClient.Create(groupName, publicKey, privateKey);
    }
}
