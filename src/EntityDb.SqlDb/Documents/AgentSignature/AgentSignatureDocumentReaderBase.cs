using System;

namespace EntityDb.SqlDb.Documents.AgentSignature;

internal abstract class AgentSignatureDocumentReaderBase
{
    private readonly string[] _propertyNames = Array.Empty<string>();

    protected readonly int _transactionIdOrdinal;
    protected readonly int _transactionTimeStampOrdinal;
    protected readonly int _entityIdsOrdinal;
    protected readonly int _dataTypeOrdinal;
    protected readonly int _dataOrdinal;

    public string[] GetPropertyNames() => _propertyNames;

    protected AgentSignatureDocumentReaderBase(string[] propertyNames)
    {
        _propertyNames = propertyNames;

        _transactionIdOrdinal = Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.TransactionId));
        _transactionTimeStampOrdinal = Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.TransactionTimeStamp));
        _entityIdsOrdinal = Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.EntityIds));
        _dataTypeOrdinal = Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.DataType));
        _dataOrdinal = Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.Data));
    }
}
