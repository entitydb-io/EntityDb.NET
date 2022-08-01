using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Command;

internal class CommandEntityVersionNumberDocumentReader : CommandDocumentReaderBase, IDocumentReader<CommandDocument>
{
    static CommandEntityVersionNumberDocumentReader()
    {
        Configure(new[]
        {
            nameof(CommandDocument.EntityVersionNumber),
        });
    }

    public async Task<CommandDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new CommandDocument
        {
            EntityVersionNumber = new VersionNumber(Convert.ToUInt64(await dbDataReader.GetFieldValueAsync<long>(_entityVersionNumberOrdinal)))
        };
    }
}
