using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Command;

internal class CommandDataDocumentReader : CommandDocumentReaderBase, IDocumentReader<CommandDocument>
{
    static CommandDataDocumentReader()
    {
        Configure(new[]
        {
            nameof(CommandDocument.Data),
        });
    }

    public async Task<CommandDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new CommandDocument
        {
            Data = await dbDataReader.GetFieldValueAsync<string>(_dataOrdinal)
        };
    }
}
