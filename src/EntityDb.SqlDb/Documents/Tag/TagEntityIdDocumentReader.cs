using EntityDb.Abstractions.ValueObjects;
using System.Data.Common;

namespace EntityDb.SqlDb.Documents.Tag;

internal class TagEntityIdDocumentReader : TagDocumentReaderBase, IDocumentReader<TagDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(TagDocument.EntityId),
    };

    public TagEntityIdDocumentReader() : base(_propertyNames)
    {
    }

    public async Task<TagDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new TagDocument
        {
            EntityId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_entityIdOrdinal))
        };
    }
}
