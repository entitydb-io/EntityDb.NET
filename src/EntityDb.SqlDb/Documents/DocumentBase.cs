using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.SqlDb.Documents;

internal abstract record DocumentBase
{
    public Guid? Id { get; init; }
    public TimeStamp TransactionTimeStamp { get; init; }
    public Id TransactionId { get; init; }
    public string DataType { get; init; } = default!;
    public string Data { get; init; } = default!;
}
