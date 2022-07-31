using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.SqlDb.Documents;

internal interface IEntityDocument : ITransactionDocument
{
    Id EntityId { get; }
    VersionNumber EntityVersionNumber { get; }
}

internal interface IEntityDocument<TDocument> : IEntityDocument, ITransactionDocument<TDocument>
{
    static abstract IDocumentReader<TDocument> EntityIdDocumentReader { get; }
}
