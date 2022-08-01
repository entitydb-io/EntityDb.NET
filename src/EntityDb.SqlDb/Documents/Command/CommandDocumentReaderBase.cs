using System;

namespace EntityDb.SqlDb.Documents.Command;

internal abstract class CommandDocumentReaderBase
{
    private readonly string[] _propertyNames = Array.Empty<string>();

    protected readonly int _transactionIdOrdinal;
    protected readonly int _transactionTimeStampOrdinal;
    protected readonly int _entityIdOrdinal;
    protected readonly int _entityVersionNumberOrdinal;
    protected readonly int _dataTypeOrdinal;
    protected readonly int _dataOrdinal;

    public string[] GetPropertyNames() => _propertyNames;

    protected CommandDocumentReaderBase(string[] propertyNames)
    {
        _propertyNames = propertyNames;

        _transactionIdOrdinal = Array.IndexOf(_propertyNames, nameof(CommandDocument.TransactionId));
        _transactionTimeStampOrdinal = Array.IndexOf(_propertyNames, nameof(CommandDocument.TransactionTimeStamp));
        _entityIdOrdinal = Array.IndexOf(_propertyNames, nameof(CommandDocument.EntityId));
        _entityVersionNumberOrdinal = Array.IndexOf(_propertyNames, nameof(CommandDocument.EntityVersionNumber));
        _dataTypeOrdinal = Array.IndexOf(_propertyNames, nameof(CommandDocument.DataType));
        _dataOrdinal = Array.IndexOf(_propertyNames, nameof(CommandDocument.Data));
    }
}
