using EntityDb.Abstractions.ValueObjects;
using System.Data.Common;

namespace EntityDb.SqlDb.Documents.Lease;

internal class LeaseEntityIdDocumentReader : LeaseDocumentReaderBase, IDocumentReader<LeaseDocument>
{
    private static readonly string[] _propertyName =
    {
        nameof(LeaseDocument.EntityId),
    };

    public LeaseEntityIdDocumentReader() : base(_propertyName)
    {
    }

    public async Task<LeaseDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new LeaseDocument
        {
            EntityId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_entityIdOrdinal))
        };
    }
}
