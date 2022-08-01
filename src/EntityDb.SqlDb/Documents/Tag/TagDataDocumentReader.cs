using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Tag;

internal class TagDataDocumentReader : TagDocumentReaderBase, IDocumentReader<TagDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(TagDocument.Data),
    };

    public TagDataDocumentReader() : base(_propertyNames)
    {
    }

    public async Task<TagDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new TagDocument
        {
            Data = await dbDataReader.GetFieldValueAsync<string>(_dataOrdinal)
        };
    }
}
