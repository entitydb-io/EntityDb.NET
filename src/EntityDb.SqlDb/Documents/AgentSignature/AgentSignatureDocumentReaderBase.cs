using System;

namespace EntityDb.SqlDb.Documents.AgentSignature;

internal abstract class AgentSignatureDocumentReaderBase
{
    private static string[] _propertyNames = Array.Empty<string>();

    protected static int _transactionIdOrdinal;
    protected static int _transactionTimeStampOrdinal;
    protected static int _entityIdsOrdinal;
    protected static int _dataTypeOrdinal;
    protected static int _dataOrdinal;

    public string[] GetPropertyNames() => _propertyNames;

    protected static void Configure(string[] propertyNames)
    {
        _propertyNames = propertyNames;

        _transactionIdOrdinal = Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.TransactionId));
        _transactionTimeStampOrdinal = Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.TransactionTimeStamp));
        _entityIdsOrdinal = Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.EntityIds));
        _dataTypeOrdinal = Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.DataType));
        _dataOrdinal = Array.IndexOf(_propertyNames, nameof(AgentSignatureDocument.Data));
    }
}
