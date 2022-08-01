using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Lease;

internal class LeaseEntityIdDocumentReader : LeaseDocumentReaderBase, IDocumentReader<LeaseDocument>
{
    static LeaseEntityIdDocumentReader()
    {
        Configure(new[]
        {
            nameof(LeaseDocument.EntityId),
        });
    }

    public async Task<LeaseDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new LeaseDocument
        {
            EntityId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_entityIdOrdinal))
        };
    }
}
