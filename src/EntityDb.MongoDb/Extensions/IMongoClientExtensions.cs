using EntityDb.MongoDb.Documents;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Extensions
{
    /// <summary>
    /// Extensions for the mongo client.
    /// </summary>
    public static class IMongoClientExtensions
    {
        /// <summary>
        /// Provisions the needed collections on the database.
        /// </summary>
        /// <param name="mongoClient">The mongo client.</param>
        /// <param name="entityName">The name of the entity, which is used as the database name.</param>
        /// <returns>An asynchronous task that, when complete, signals that the collections have been provisioned.</returns>
        /// <remarks>
        /// You should ONLY use this in your code for integration testing. Real databases should be provisioned using the dotnet tool EntityDb.MongoDb.Provisioner.
        /// </remarks>
        public static async Task ProvisionCollections(this IMongoClient mongoClient, string entityName)
        {
            var mongoDatabase = mongoClient.GetDatabase(entityName);

            await SourceDocument.ProvisionCollection(mongoDatabase);
            await CommandDocument.ProvisionCollection(mongoDatabase);
            await FactDocument.ProvisionCollection(mongoDatabase);
            await LeaseDocument.ProvisionCollection(mongoDatabase);
        }
    }
}
