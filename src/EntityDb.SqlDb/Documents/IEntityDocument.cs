namespace EntityDb.SqlDb.Documents;

internal interface IEntityDocument : Common.Documents.IEntityDocument<string>, ITransactionDocument
{
}

internal interface IEntityDocument<TDocument> : IEntityDocument, ITransactionDocument<TDocument>
    where TDocument : IEntityDocument
{
    static abstract IDocumentReader<TDocument> EntityIdDocumentReader { get; }
}
