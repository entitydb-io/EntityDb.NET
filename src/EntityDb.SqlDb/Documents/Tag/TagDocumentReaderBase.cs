namespace EntityDb.SqlDb.Documents.Tag;

internal abstract class TagDocumentReaderBase
{
    private readonly string[] _propertyNames;

    protected readonly int _transactionIdOrdinal;
    protected readonly int _transactionTimeStampOrdinal;
    protected readonly int _entityIdOrdinal;
    protected readonly int _entityVersionNumberOrdinal;
    protected readonly int _dataTypeOrdinal;
    protected readonly int _dataOrdinal;
    protected readonly int _labelOrdinal;
    protected readonly int _valueOrdinal;

    public string[] GetPropertyNames() => _propertyNames;

    protected TagDocumentReaderBase(string[] propertyNames)
    {
        _propertyNames = propertyNames;

        _transactionIdOrdinal = Array.IndexOf(_propertyNames, nameof(TagDocument.TransactionId));
        _transactionTimeStampOrdinal = Array.IndexOf(_propertyNames, nameof(TagDocument.TransactionTimeStamp));
        _entityIdOrdinal = Array.IndexOf(_propertyNames, nameof(TagDocument.EntityId));
        _entityVersionNumberOrdinal = Array.IndexOf(_propertyNames, nameof(TagDocument.EntityVersionNumber));
        _dataTypeOrdinal = Array.IndexOf(_propertyNames, nameof(TagDocument.DataType));
        _dataOrdinal = Array.IndexOf(_propertyNames, nameof(TagDocument.Data));
        _labelOrdinal = Array.IndexOf(_propertyNames, nameof(TagDocument.Label));
        _valueOrdinal = Array.IndexOf(_propertyNames, nameof(TagDocument.Value));
    }
}
