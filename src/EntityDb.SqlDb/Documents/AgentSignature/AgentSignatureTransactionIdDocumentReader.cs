﻿using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.AgentSignature;

internal class AgentSignatureTransactionIdDocumentReader : AgentSignatureDocumentReaderBase, IDocumentReader<AgentSignatureDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(AgentSignatureDocument.TransactionId),
    };

    public AgentSignatureTransactionIdDocumentReader() : base(_propertyNames)
    {
    }

    public async Task<AgentSignatureDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new AgentSignatureDocument
        {
            TransactionId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_transactionIdOrdinal))
        };
    }
}
