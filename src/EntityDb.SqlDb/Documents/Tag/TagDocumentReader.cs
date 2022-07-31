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

    public string[] GetPropertyNames() => _propertyNames;

    public async Task<TagDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new TagDocument
        {
            TransactionId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(0)),
            TransactionTimeStamp = new TimeStamp(await dbDataReader.GetFieldValueAsync<DateTime>(1)),
            EntityId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(2)),
            EntityVersionNumber = new VersionNumber(Convert.ToUInt64(await dbDataReader.GetFieldValueAsync<long>(3))),
            DataType = await dbDataReader.GetFieldValueAsync<string>(4),
            Data = await dbDataReader.GetFieldValueAsync<string>(5),
            Label = await dbDataReader.GetFieldValueAsync<string>(6),
            Value = await dbDataReader.GetFieldValueAsync<string>(7),
        };
    }
}
