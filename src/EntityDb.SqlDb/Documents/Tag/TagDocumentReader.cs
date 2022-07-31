using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Tag;

internal class TagDocumentReader : IDocumentReader<TagDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(TagDocument.TransactionId),
        nameof(TagDocument.TransactionTimeStamp),
        nameof(TagDocument.EntityId),
        nameof(TagDocument.EntityVersionNumber),
        nameof(TagDocument.DataType),
        nameof(TagDocument.Data),
        nameof(TagDocument.Label),
        nameof(TagDocument.Value),
    };

    private static readonly int _transactionIdOrdinal
        = Array.IndexOf(_propertyNames, nameof(TagDocument.TransactionId));

    private static readonly int _transactionTimeStampOrdinal
        = Array.IndexOf(_propertyNames, nameof(TagDocument.TransactionTimeStamp));

    private static readonly int _entityIdOrdinal
        = Array.IndexOf(_propertyNames, nameof(TagDocument.EntityId));

    private static readonly int _entityVersionNumberOrdinal
        = Array.IndexOf(_propertyNames, nameof(TagDocument.EntityVersionNumber));

    private static readonly int _dataTypeOrdinal
        = Array.IndexOf(_propertyNames, nameof(TagDocument.DataType));

    private static readonly int _dataOrdinal
        = Array.IndexOf(_propertyNames, nameof(TagDocument.Data));

    private static readonly int _labelOrdinal
        = Array.IndexOf(_propertyNames, nameof(TagDocument.Label));

    private static readonly int _valueOrdinal
        = Array.IndexOf(_propertyNames, nameof(TagDocument.Value));

    public string[] GetPropertyNames() => _propertyNames;

    public async Task<TagDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new TagDocument
        {
            TransactionId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_transactionIdOrdinal)),
            TransactionTimeStamp = new TimeStamp(await dbDataReader.GetFieldValueAsync<DateTime>(_transactionTimeStampOrdinal)),
            EntityId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_entityIdOrdinal)),
            EntityVersionNumber = new VersionNumber(Convert.ToUInt64(await dbDataReader.GetFieldValueAsync<long>(_entityVersionNumberOrdinal))),
            DataType = await dbDataReader.GetFieldValueAsync<string>(_dataTypeOrdinal),
            Data = await dbDataReader.GetFieldValueAsync<string>(_dataOrdinal),
            Label = await dbDataReader.GetFieldValueAsync<string>(_labelOrdinal),
            Value = await dbDataReader.GetFieldValueAsync<string>(_valueOrdinal),
        };
    }
}
