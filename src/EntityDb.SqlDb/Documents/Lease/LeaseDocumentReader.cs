using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Lease;

internal class LeaseDocumentReader : IDocumentReader<LeaseDocument>
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

    private static readonly int _transactionIdOrdinal
        = Array.IndexOf(_propertyNames, nameof(LeaseDocument.TransactionId));

    private static readonly int _transactionTimeStampOrdinal
        = Array.IndexOf(_propertyNames, nameof(LeaseDocument.TransactionTimeStamp));

    private static readonly int _entityIdOrdinal
        = Array.IndexOf(_propertyNames, nameof(LeaseDocument.EntityId));

    private static readonly int _entityVersionNumberOrdinal
        = Array.IndexOf(_propertyNames, nameof(LeaseDocument.EntityVersionNumber));

    private static readonly int _dataTypeOrdinal
        = Array.IndexOf(_propertyNames, nameof(LeaseDocument.DataType));

    private static readonly int _dataOrdinal
        = Array.IndexOf(_propertyNames, nameof(LeaseDocument.Data));

    private static readonly int _scopeOrdinal
        = Array.IndexOf(_propertyNames, nameof(LeaseDocument.Scope));

    private static readonly int _labelOrdinal
        = Array.IndexOf(_propertyNames, nameof(LeaseDocument.Label));

    private static readonly int _valueOrdinal
        = Array.IndexOf(_propertyNames, nameof(LeaseDocument.Value));

    public string[] GetPropertyNames() => _propertyNames;

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
