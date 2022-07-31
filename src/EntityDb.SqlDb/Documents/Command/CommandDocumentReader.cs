using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Command;

internal class CommandDocumentReader : IDocumentReader<CommandDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(CommandDocument.TransactionId),
        nameof(CommandDocument.TransactionTimeStamp),
        nameof(CommandDocument.EntityId),
        nameof(CommandDocument.EntityVersionNumber),
        nameof(CommandDocument.DataType),
        nameof(CommandDocument.Data),
    };

    public string[] GetPropertyNames() => _propertyNames;

    public async Task<CommandDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new CommandDocument
        {
            TransactionId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(0)),
            TransactionTimeStamp = new TimeStamp(await dbDataReader.GetFieldValueAsync<DateTime>(1)),
            EntityId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(2)),
            EntityVersionNumber = new VersionNumber(Convert.ToUInt64(await dbDataReader.GetFieldValueAsync<long>(3))),
            DataType = await dbDataReader.GetFieldValueAsync<string>(4),
            Data = await dbDataReader.GetFieldValueAsync<string>(5),
        };
    }
}
