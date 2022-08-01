using System;

namespace EntityDb.SqlDb.Documents.Command;

internal abstract class CommandDocumentReaderBase
{
    private static string[] _propertyNames = Array.Empty<string>();

    protected static int _transactionIdOrdinal;
    protected static int _transactionTimeStampOrdinal;
    protected static int _entityIdOrdinal;
    protected static int _entityVersionNumberOrdinal;
    protected static int _dataTypeOrdinal;
    protected static int _dataOrdinal;

    public string[] GetPropertyNames() => _propertyNames;

    protected static void Configure(string[] propertyNames)
    {
        _propertyNames = propertyNames;

        _transactionIdOrdinal = Array.IndexOf(_propertyNames, nameof(CommandDocument.TransactionId));
        _transactionTimeStampOrdinal = Array.IndexOf(_propertyNames, nameof(CommandDocument.TransactionTimeStamp));
        _entityIdOrdinal = Array.IndexOf(_propertyNames, nameof(CommandDocument.EntityId));
        _entityVersionNumberOrdinal = Array.IndexOf(_propertyNames, nameof(CommandDocument.EntityId));
        _dataTypeOrdinal = Array.IndexOf(_propertyNames, nameof(CommandDocument.DataType));
        _dataOrdinal = Array.IndexOf(_propertyNames, nameof(CommandDocument.Data));
    }
}
