using System;

namespace EntityDb.SqlDb.Documents.Lease;

internal abstract class LeaseDocumentReaderBase
{
    private readonly string[] _propertyNames;

    protected readonly int _transactionIdOrdinal;
    protected readonly int _transactionTimeStampOrdinal;
    protected readonly int _entityIdOrdinal;
    protected readonly int _entityVersionNumberOrdinal;
    protected readonly int _dataTypeOrdinal;
    protected readonly int _dataOrdinal;
    protected readonly int _scopeOrdinal;
    protected readonly int _labelOrdinal;
    protected readonly int _valueOrdinal;

    public string[] GetPropertyNames() => _propertyNames;

    protected LeaseDocumentReaderBase(string[] propertyNames)
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
