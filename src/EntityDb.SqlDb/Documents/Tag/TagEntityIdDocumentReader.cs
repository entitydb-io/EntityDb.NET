using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Tag;

internal class TagEntityIdDocumentReader : IDocumentReader<TagDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(TagDocument.EntityId),
    };

    private static readonly int _entityIdOrdinal
        = Array.IndexOf(_propertyNames, nameof(TagDocument.EntityId));

    public string[] GetPropertyNames() => _propertyNames;

    public async Task<TagDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new TagDocument
        {
            EntityId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_entityIdOrdinal))
        };
    }
}
