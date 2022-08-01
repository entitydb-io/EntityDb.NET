using EntityDb.Abstractions.ValueObjects;
using System.Data.Common;

namespace EntityDb.SqlDb.Documents.Command;

internal class CommandTransactionIdDocumentReader : CommandDocumentReaderBase, IDocumentReader<CommandDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(CommandDocument.TransactionId),
    };

    public CommandTransactionIdDocumentReader() : base(_propertyNames)
    {
    }

    public async Task<CommandDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new CommandDocument
        {
            TransactionId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_transactionIdOrdinal))
        };
    }
}
