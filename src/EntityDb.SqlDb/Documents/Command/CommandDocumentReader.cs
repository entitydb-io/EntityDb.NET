using EntityDb.Abstractions.ValueObjects;
using System.Data.Common;

namespace EntityDb.SqlDb.Documents.Command;

internal class CommandDocumentReader : CommandDocumentReaderBase, IDocumentReader<CommandDocument>
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

    public CommandDocumentReader() : base(_propertyNames)
    {
    }

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
