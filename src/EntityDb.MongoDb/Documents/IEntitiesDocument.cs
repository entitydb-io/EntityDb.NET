using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.MongoDb.Documents;

internal interface IEntitiesDocument : ITransactionDocument
{
    Id[] EntityIds { get; }
}
