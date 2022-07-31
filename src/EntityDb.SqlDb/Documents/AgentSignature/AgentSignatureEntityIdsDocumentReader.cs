using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.AgentSignature;

internal class AgentSignatureEntityIdsDocumentReader : IDocumentReader<AgentSignatureDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(AgentSignatureDocument.EntityIds),
    };

    private static readonly int _entityIdsOrdinal =
        Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.EntityIds));

    public string[] GetPropertyNames() => _propertyNames;

    public async Task<AgentSignatureDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new AgentSignatureDocument
        {
            EntityIds = (await dbDataReader.GetFieldValueAsync<Guid[]>(_entityIdsOrdinal))
                .Select(guid => new Id(guid))
                .ToArray()
        };
    }
}
