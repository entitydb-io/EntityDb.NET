using EntityDb.Abstractions.ValueObjects;
using System.Data.Common;

namespace EntityDb.SqlDb.Documents.Tag;

internal class TagDocumentReader : TagDocumentReaderBase, IDocumentReader<TagDocument>
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

    public TagDocumentReader() : base(_propertyNames)
    {
    }

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
