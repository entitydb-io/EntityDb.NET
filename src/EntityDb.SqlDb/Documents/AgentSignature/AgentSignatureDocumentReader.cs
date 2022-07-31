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

    private static readonly int _transactionIdOrdinal =
        Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.TransactionId));
    
    private static readonly int _transactionTimeStampOrdinal =
        Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.TransactionTimeStamp));
    
    private static readonly int _entityIdsOrdinal =
        Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.EntityIds));
    
    private static readonly int _dataTypeOrdinal =
        Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.DataType));

    private static readonly int _dataOrdinal =
        Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.Data));

    public string[] GetPropertyNames() => _propertyNames;

    public async Task<AgentSignatureDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new AgentSignatureDocument
        {
            TransactionId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(_transactionIdOrdinal)),
            TransactionTimeStamp = new TimeStamp(await dbDataReader.GetFieldValueAsync<DateTime>(_transactionTimeStampOrdinal)),
            EntityIds = (await dbDataReader.GetFieldValueAsync<Guid[]>(_entityIdsOrdinal))
                .Select(guid => new Id(guid))
                .ToArray(),
            DataType = await dbDataReader.GetFieldValueAsync<string>(_dataTypeOrdinal),
            Data = await dbDataReader.GetFieldValueAsync<string>(_dataOrdinal),
        };
    }
}
