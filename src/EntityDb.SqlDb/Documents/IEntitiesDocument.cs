namespace EntityDb.SqlDb.Documents;

internal interface IEntitiesDocument : Common.Documents.IEntitiesDocument<string>, ITransactionDocument
{
}

internal interface IEntitiesDocument<TDocument> : IEntitiesDocument, ITransactionDocument<TDocument>
    where TDocument : IEntitiesDocument
{
    static abstract IDocumentReader<TDocument> EntityIdsDocumentReader { get; }
}
