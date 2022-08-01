using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Documents;

internal interface ITransactionDocument<TSerializedData>
{
    Id TransactionId { get; }

    TimeStamp TransactionTimeStamp { get; }

    string DataType { get; }

    TSerializedData Data { get; }
}
