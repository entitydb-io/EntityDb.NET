using System.Data.Common;

namespace EntityDb.SqlDb.Documents.Lease;

internal class LeaseDataDocumentReader : LeaseDocumentReaderBase, IDocumentReader<LeaseDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(LeaseDocument.Data),
    };

    public LeaseDataDocumentReader() : base(_propertyNames)
    {
    }

    public async Task<LeaseDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new LeaseDocument
        {
            Data = await dbDataReader.GetFieldValueAsync<string>(_dataOrdinal)
        };
    }
}
