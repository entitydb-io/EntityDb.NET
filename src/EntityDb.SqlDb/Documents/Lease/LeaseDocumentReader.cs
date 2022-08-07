using EntityDb.Abstractions.ValueObjects;
using System.Data.Common;

namespace EntityDb.SqlDb.Documents.Lease;

internal class LeaseDocumentReader : LeaseDocumentReaderBase, IDocumentReader<LeaseDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(LeaseDocument.TransactionId),
        nameof(LeaseDocument.TransactionTimeStamp),
        nameof(LeaseDocument.EntityId),
        nameof(LeaseDocument.EntityVersionNumber),
        nameof(LeaseDocument.DataType),
        nameof(LeaseDocument.Data),
        nameof(LeaseDocument.Scope),
        nameof(LeaseDocument.Label),
        nameof(LeaseDocument.Value),
    };

    public LeaseDocumentReader() : base(_propertyNames)
    {
    }

    public async Task<LeaseDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new LeaseDocument
        {
            TransactionId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_transactionIdOrdinal)),
            TransactionTimeStamp = new TimeStamp(await dbDataReader.GetFieldValueAsync<DateTime>(_transactionTimeStampOrdinal)),
            EntityId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_entityIdOrdinal)),
            EntityVersionNumber = new VersionNumber(Convert.ToUInt64(await dbDataReader.GetFieldValueAsync<long>(_entityVersionNumberOrdinal))),
            DataType = await dbDataReader.GetFieldValueAsync<string>(_dataTypeOrdinal),
            Data = await dbDataReader.GetFieldValueAsync<string>(_dataOrdinal),
            Scope = await dbDataReader.GetFieldValueAsync<string>(_scopeOrdinal),
            Label = await dbDataReader.GetFieldValueAsync<string>(_labelOrdinal),
            Value = await dbDataReader.GetFieldValueAsync<string>(_valueOrdinal),
        };
    }
}
