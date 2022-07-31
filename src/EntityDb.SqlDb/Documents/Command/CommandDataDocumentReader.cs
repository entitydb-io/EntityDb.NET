using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Command;

internal class CommandDataDocumentReader : IDocumentReader<CommandDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(CommandDocument.Data),
    };

    private static readonly int _dataOrdinal =
        Array.IndexOf(_propertyNames, nameof(CommandDocument.Data));

    public string[] GetPropertyNames() => _propertyNames;

    public async Task<CommandDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new CommandDocument
        {
            Data = await dbDataReader.GetFieldValueAsync<string>(_dataOrdinal)
        };
    }
}
