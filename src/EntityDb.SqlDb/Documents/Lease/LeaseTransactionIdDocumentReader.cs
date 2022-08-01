using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Lease;

internal class LeaseTransactionIdDocumentReader : LeaseDocumentReaderBase, IDocumentReader<LeaseDocument>
{
    static LeaseTransactionIdDocumentReader()
    {
        Configure(new[]
        {
            nameof(LeaseDocument.TransactionId),
        });
    }

    public async Task<LeaseDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new LeaseDocument
        {
            TransactionId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_transactionIdOrdinal))
        };
    }
}
