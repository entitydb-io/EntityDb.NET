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

    private static readonly int _transactionIdOrdinal
        = Array.IndexOf(_propertyNames, nameof(CommandDocument.TransactionId));

    private static readonly int _transactionTimeStampOrdinal
        = Array.IndexOf(_propertyNames, nameof(CommandDocument.TransactionTimeStamp));

    private static readonly int _entityIdOrdinal
        = Array.IndexOf(_propertyNames, nameof(CommandDocument.EntityId));

    private static readonly int _entityVersionNumberOrdinal
        = Array.IndexOf(_propertyNames, nameof(CommandDocument.EntityVersionNumber));

    private static readonly int _dataTypeOrdinal
        = Array.IndexOf(_propertyNames, nameof(CommandDocument.DataType));

    private static readonly int _dataOrdinal
        = Array.IndexOf(_propertyNames, nameof(CommandDocument.Data));

    public string[] GetPropertyNames() => _propertyNames;

    public async Task<CommandDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new CommandDocument
        {
            TransactionId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_transactionIdOrdinal)),
            TransactionTimeStamp = new TimeStamp(await dbDataReader.GetFieldValueAsync<DateTime>(_transactionTimeStampOrdinal)),
            EntityId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_entityIdOrdinal)),
            EntityVersionNumber = new VersionNumber(Convert.ToUInt64(await dbDataReader.GetFieldValueAsync<long>(_entityVersionNumberOrdinal))),
            DataType = await dbDataReader.GetFieldValueAsync<string>(_dataTypeOrdinal),
            Data = await dbDataReader.GetFieldValueAsync<string>(_dataOrdinal),
        };
    }
}
