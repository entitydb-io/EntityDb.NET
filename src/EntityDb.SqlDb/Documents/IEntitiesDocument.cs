using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.SqlDb.Documents;

internal interface IEntitiesDocument : ITransactionDocument
{
    Id[] EntityIds { get; }
}

internal interface IEntitiesDocument<TDocument> : IEntitiesDocument, ITransactionDocument<TDocument>
    where TDocument : IEntitiesDocument
{
    static abstract IDocumentReader<TDocument> EntityIdsDocumentReader { get; }
}
