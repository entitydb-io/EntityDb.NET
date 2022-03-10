using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.MongoDb.Documents;

internal interface IEntityDocument : ITransactionDocument
{
    Id EntityId { get; }
    VersionNumber EntityVersionNumber { get; }
}
