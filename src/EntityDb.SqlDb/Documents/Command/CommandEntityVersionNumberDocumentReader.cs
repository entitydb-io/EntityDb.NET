using EntityDb.Abstractions.ValueObjects;
using System.Data.Common;

namespace EntityDb.SqlDb.Documents.Command;

internal class CommandEntityVersionNumberDocumentReader : CommandDocumentReaderBase, IDocumentReader<CommandDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(CommandDocument.EntityVersionNumber),
    };

    public CommandEntityVersionNumberDocumentReader() : base(_propertyNames)
    {
    }

    public async Task<CommandDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new CommandDocument
        {
            EntityVersionNumber = new VersionNumber(Convert.ToUInt64(await dbDataReader.GetFieldValueAsync<long>(_entityVersionNumberOrdinal)))
        };
    }
}
