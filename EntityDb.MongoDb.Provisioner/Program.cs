using EntityDb.MongoDb.Documents;
using EntityDb.MongoDb.Extensions;
using EntityDb.MongoDb.Provisioner.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner
{

    public class Program
    {
        private static readonly IConfiguration _configuration;

        static Program()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddUserSecrets<Program>()
                .Build();
        }

        public static Task<int> Main()
        {
            Console.Write("Please enter args: ");

            var input = Console.ReadLine() ?? string.Empty;

            var rootCommand = new RootCommand();

            AddCommands(rootCommand);

            return rootCommand.InvokeAsync(input.Split(' '));
        }

        private static readonly Regex entityNameRegex = new("^[a-z][a-z]*$", RegexOptions.IgnoreCase);

        private static void AddCommands(RootCommand rootCommand)
        {
            var entityNameArgument = new Argument<string>("entity-name", "The name of the entity being provisioned.");
            var entityPasswordArgument = new Argument<string>("entity-password", "The password for the entity service user.");

            entityNameArgument.AddValidator((entityNameResult) =>
            {
                var entityName = entityNameResult.GetValueOrDefault<string>() ?? string.Empty;

                if (entityNameRegex.IsMatch(entityName))
                {
                    return null;
                }

                return "The entity name must begin with an letter, and can only contain letters.";
            });

            var createRole = new Command("create-role");

            createRole.AddArgument(entityNameArgument);

            createRole.Handler = CommandHandler.Create(async (string entityName, string groupId, string publicKey, string privateKey) =>
            {
                await CreateRole(entityName);
            });

            rootCommand.AddCommand(createRole);

            // --

            var createUser = new Command("create-user");

            createUser.AddArgument(entityNameArgument);

            createUser.Handler = CommandHandler.Create(async (string entityName) =>
            {
                await CreateUser(entityName);
            });

            rootCommand.AddCommand(createUser);

            // --

            var provisionCollections = new Command("provision-collections");

            provisionCollections.AddArgument(entityNameArgument);
            provisionCollections.AddArgument(entityPasswordArgument);

            provisionCollections.Handler = CommandHandler.Create(async (string entityName, string entityPassword) =>
            {
                await ProvisionCollections(entityName, entityPassword);
            });

            rootCommand.AddCommand(provisionCollections);
        }

        private static MongoDbAtlasClient GetClient()
        {
            var mongoDbAtlasSettings = _configuration.GetSection("MongoDbAtlasSettings").Get<Settings>();

            return new MongoDbAtlasClient(mongoDbAtlasSettings);
        }

        private static string GetEntityServiceUserName(string entityName)
        {
            return $"{entityName}ServiceUser";
        }

        private static string GetEntityServiceRoleName(string entityName)
        {
            return $"{entityName}ServiceRole";
        }

        private static async Task CreateRole(string entityName)
        {
            var entityServiceRoleName = GetEntityServiceRoleName(entityName);

            using var mongoDbAtlasClient = GetClient();

            if (await mongoDbAtlasClient.RoleExists(entityServiceRoleName))
            {
                return;
            }

            var allClusterResources = new MongoDbAtlasResource
            {
                Cluster = true,
            };

            var allDbResources = new MongoDbAtlasResource
            {
                Db = entityName,
            };

            var sourceResource = new MongoDbAtlasResource
            {
                Db = entityName,
                Collection = SourceDocument.CollectionName,
            };

            var commandResource = new MongoDbAtlasResource
            {
                Db = entityName,
                Collection = CommandDocument.CollectionName,
            };

            var factResource = new MongoDbAtlasResource
            {
                Db = entityName,
                Collection = FactDocument.CollectionName,
            };

            var tagResource = new MongoDbAtlasResource
            {
                Db = entityName,
                Collection = TagDocument.CollectionName,
            };

            var roleActions = new[]
            {
                new MongoDbAtlasRoleAction
                {
                    Action = "LIST_DATABASES",
                    Resources = new[]
                    {
                        allClusterResources,
                    },
                },
                new MongoDbAtlasRoleAction
                {
                    Action = "LIST_COLLECTIONS",
                    Resources = new[]
                    {
                        allDbResources,
                    },
                },
                new MongoDbAtlasRoleAction
                {
                    Action = "FIND",
                    Resources = new[]
                    {
                        sourceResource,
                        commandResource,
                        factResource,
                        tagResource,
                    },
                },
                new MongoDbAtlasRoleAction
                {
                    Action = "INSERT",
                    Resources = new[]
                    {
                        sourceResource,
                        commandResource,
                        factResource,
                        tagResource,
                    },
                },
                new MongoDbAtlasRoleAction
                {
                    Action = "CREATE_INDEX",
                    Resources = new[]
                    {
                        sourceResource,
                        commandResource,
                        factResource,
                        tagResource,
                    },
                },
                new MongoDbAtlasRoleAction
                {
                    Action = "REMOVE",
                    Resources = new[]
                    {
                        tagResource,
                    },
                },
            };

            await mongoDbAtlasClient.CreateRole(entityServiceRoleName, roleActions);
        }

        private static async Task CreateUser(string entityName)
        {
            var entityServiceRoleName = GetEntityServiceRoleName(entityName);
            var entityServiceUserName = GetEntityServiceUserName(entityName);

            using var mongoDbAtlasClient = GetClient();

            if (await mongoDbAtlasClient.UserExists("admin", entityServiceUserName))
            {
                return;
            }

            var entityServicePassword = Guid.NewGuid().ToString();

            Console.WriteLine("Password: " + entityServicePassword);

            var roles = new[]
            {
                new MongoDbAtlastUserRole
                {
                    DatabaseName = "admin",
                    RoleName = entityServiceRoleName,
                },
            };

            await mongoDbAtlasClient.CreateUser("admin", entityServiceUserName, entityServicePassword, roles);
        }

        private static async Task ProvisionCollections(string entityName, string entityServicePassword)
        {
            var entityServiceUserName = GetEntityServiceUserName(entityName);

            var mongoClient = new MongoClient($"mongodb+srv://{entityServiceUserName}:{entityServicePassword}@development-1.4i6ff.mongodb.net/admin");

            await mongoClient.ProvisionCollections(entityName);
        }
    }
}
