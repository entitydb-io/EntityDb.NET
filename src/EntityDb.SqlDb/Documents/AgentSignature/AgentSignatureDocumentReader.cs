using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.AgentSignature;

internal class AgentSignatureDocumentReader : IDocumentReader<AgentSignatureDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(AgentSignatureDocument.TransactionId),
        nameof(AgentSignatureDocument.TransactionTimeStamp),
        nameof(AgentSignatureDocument.EntityIds),
        nameof(AgentSignatureDocument.DataType),
        nameof(AgentSignatureDocument.Data),
    };

    public string[] GetPropertyNames() => _propertyNames;

    public async Task<AgentSignatureDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new AgentSignatureDocument
        {
            TransactionId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(0)),
            TransactionTimeStamp = new TimeStamp(await dbDataReader.GetFieldValueAsync<DateTime>(1)),
            EntityIds = (await dbDataReader.GetFieldValueAsync<Guid[]>(2))
                .Select(guid => new Id(guid))
                .ToArray(),
            DataType = await dbDataReader.GetFieldValueAsync<string>(3),
            Data = await dbDataReader.GetFieldValueAsync<string>(4),
        };
    }
}
