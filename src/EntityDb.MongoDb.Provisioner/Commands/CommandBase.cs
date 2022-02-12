using EntityDb.MongoDb.Provisioner.MongoDbAtlas;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner.Commands
{
    internal abstract class CommandBase
    {
        private static readonly Regex entityNameRegex = new("^[a-z][a-z]*$", RegexOptions.IgnoreCase);

        protected static void AddMongoDbAtlasArgumentsTo(Command command)
        {
            var groupNameArgument = new Argument<string>("group-name",
                "The name of the MongoDb Atlas Group/Project to access via API.");
            var publicKeyArgument = new Argument<string>("public-key", "The public key used to authenticate via API.");
            var privateKeyArgument =
                new Argument<string>("private-key", "The private key used to authenticate via API.");

            command.AddArgument(groupNameArgument);
            command.AddArgument(publicKeyArgument);
            command.AddArgument(privateKeyArgument);
        }

        protected static void AddClusterNameArgumentTo(Command command)
        {
            var clusterNameArgument = new Argument<string>("cluster-name",
                "The name of the Cluster on which the entity will be provisioned.");

            command.AddArgument(clusterNameArgument);
        }

        protected static void AddEntityNameArgumentTo(Command command)
        {
            var entityNameArgument = new Argument<string>("entity-name", "The name of the entity being provisioned.");

            entityNameArgument.AddValidator(entityNameResult =>
            {
                var entityName = entityNameResult.GetValueOrDefault<string>() ?? string.Empty;

                if (entityNameRegex.IsMatch(entityName))
                {
                    return null;
                }

                return "The entity name must begin with an letter, and can only contain letters.";
            });

            command.AddArgument(entityNameArgument);
        }

        protected static void AddEntityPasswordArgumentTo(Command command)
        {
            var entityPasswordArgument =
                new Argument<string>("entity-password", "The password for the entity service user.");

            command.AddArgument(entityPasswordArgument);
        }

        internal static Task<MongoDbAtlasClient> GetMongoDbAtlasClient(string groupName, string publicKey,
            string privateKey)
        {
            return MongoDbAtlasClient.Create(groupName, publicKey, privateKey);
        }
    }
}
