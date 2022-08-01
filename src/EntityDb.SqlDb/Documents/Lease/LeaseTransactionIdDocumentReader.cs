using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Lease;

internal class LeaseTransactionIdDocumentReader : LeaseDocumentReaderBase, IDocumentReader<LeaseDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(LeaseDocument.TransactionId),
    };

    public LeaseTransactionIdDocumentReader() : base(_propertyNames)
    {
    }

    public async Task<LeaseDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new LeaseDocument
        {
            TransactionId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_transactionIdOrdinal))
        };
    }
}
