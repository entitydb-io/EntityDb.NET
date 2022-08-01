using System;

namespace EntityDb.SqlDb.Documents.Lease;

internal abstract class LeaseDocumentReaderBase
{
    private static string[] _propertyNames = Array.Empty<string>();

    protected static int _transactionIdOrdinal;
    protected static int _transactionTimeStampOrdinal;
    protected static int _entityIdOrdinal;
    protected static int _entityVersionNumberOrdinal;
    protected static int _dataTypeOrdinal;
    protected static int _dataOrdinal;
    protected static int _scopeOrdinal;
    protected static int _labelOrdinal;
    protected static int _valueOrdinal;

    public string[] GetPropertyNames() => _propertyNames;

    protected static void Configure(string[] propertyNames)
    {
        _propertyNames = propertyNames;

        _transactionIdOrdinal = Array.IndexOf(_propertyNames, nameof(LeaseDocument.TransactionId));
        _transactionTimeStampOrdinal = Array.IndexOf(_propertyNames, nameof(LeaseDocument.TransactionTimeStamp));
        _entityIdOrdinal = Array.IndexOf(_propertyNames, nameof(LeaseDocument.EntityId));
        _entityVersionNumberOrdinal = Array.IndexOf(_propertyNames, nameof(LeaseDocument.EntityId));
        _dataTypeOrdinal = Array.IndexOf(_propertyNames, nameof(LeaseDocument.DataType));
        _dataOrdinal = Array.IndexOf(_propertyNames, nameof(LeaseDocument.Data));
        _scopeOrdinal = Array.IndexOf(_propertyNames, nameof(LeaseDocument.Scope));
        _labelOrdinal = Array.IndexOf(_propertyNames, nameof(LeaseDocument.Label));
        _valueOrdinal = Array.IndexOf(_propertyNames, nameof(LeaseDocument.Value));
    }
}
