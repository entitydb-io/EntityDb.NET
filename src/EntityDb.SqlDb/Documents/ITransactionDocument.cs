using EntityDb.Abstractions.ValueObjects;
using System;
using System.Collections.Generic;

namespace EntityDb.SqlDb.Documents;

internal interface ITransactionDocument
{

    Guid? Id { get; }

    Id TransactionId { get; }

    TimeStamp TransactionTimeStamp { get; }

    string DataType { get; }

    string Data { get; }

    Dictionary<string, object> ToDictionary();
}

internal interface ITransactionDocument<TDocument> : ITransactionDocument
{
    static abstract IDocumentReader<TDocument> DocumentReader { get; }
    static abstract IDocumentReader<TDocument> TransactionIdDocumentReader { get; }
    static abstract IDocumentReader<TDocument> DataDocumentReader { get; }
}
