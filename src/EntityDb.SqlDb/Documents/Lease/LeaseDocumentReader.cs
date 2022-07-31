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

    public string[] GetPropertyNames() => _propertyNames;

    public async Task<LeaseDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new LeaseDocument
        {
            TransactionId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(0)),
            TransactionTimeStamp = new TimeStamp(await dbDataReader.GetFieldValueAsync<DateTime>(1)),
            EntityId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(2)),
            EntityVersionNumber = new VersionNumber(Convert.ToUInt64(await dbDataReader.GetFieldValueAsync<long>(3))),
            DataType = await dbDataReader.GetFieldValueAsync<string>(4),
            Data = await dbDataReader.GetFieldValueAsync<string>(5),
            Scope = await dbDataReader.GetFieldValueAsync<string>(6),
            Label = await dbDataReader.GetFieldValueAsync<string>(7),
            Value = await dbDataReader.GetFieldValueAsync<string>(8),
        };
    }
}
