using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Lease;

internal class LeaseDataDocumentReader : LeaseDocumentReaderBase, IDocumentReader<LeaseDocument>
{
    static LeaseDataDocumentReader()
    {
        Configure(new[]
        {
            nameof(LeaseDocument.Data),
        });
    }

    public async Task<LeaseDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new LeaseDocument
        {
            Data = await dbDataReader.GetFieldValueAsync<string>(_dataOrdinal)
        };
    }
}
