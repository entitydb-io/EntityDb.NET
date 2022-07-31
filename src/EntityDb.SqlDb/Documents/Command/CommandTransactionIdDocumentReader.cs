using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Command;

internal class CommandTransactionIdDocumentReader : IDocumentReader<CommandDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(CommandDocument.TransactionId),
    };

    private static readonly int _transactionIdOrdinal
        = Array.IndexOf(_propertyNames, nameof(CommandDocument.TransactionId));

    public string[] GetPropertyNames() => _propertyNames;

    public async Task<CommandDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new CommandDocument
        {
            TransactionId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_transactionIdOrdinal))
        };
    }
}
