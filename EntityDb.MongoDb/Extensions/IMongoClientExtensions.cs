using EntityDb.MongoDb.Documents;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Extensions
{
    internal static class IMongoClientExtensions
    {
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
