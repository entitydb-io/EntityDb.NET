namespace EntityDb.SqlDb.Documents;

internal interface ITransactionDocument : Common.Documents.ITransactionDocument<string>
{
    static abstract string TableName { get; }

    Guid? Id { get; }

    Dictionary<string, object> ToDictionary();
}

internal interface ITransactionDocument<TDocument> : ITransactionDocument
    where TDocument : ITransactionDocument
{
    static abstract IDocumentReader<TDocument> DocumentReader { get; }
    static abstract IDocumentReader<TDocument> TransactionIdDocumentReader { get; }
    static abstract IDocumentReader<TDocument> DataDocumentReader { get; }
}
