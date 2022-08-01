using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Command;

internal class CommandEntityIdDocumentReader : CommandDocumentReaderBase, IDocumentReader<CommandDocument>
{
    static CommandEntityIdDocumentReader()
    {
        Configure(new[]
        {
            nameof(CommandDocument.EntityId),
        });
    }

    public async Task<CommandDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new CommandDocument
        {
            EntityId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_entityIdOrdinal))
        };
    }
}
