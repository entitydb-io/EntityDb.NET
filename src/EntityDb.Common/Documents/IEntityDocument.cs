using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Documents;

internal interface IEntityDocument<out TSerializedData> : ITransactionDocument<TSerializedData>
{
    Id EntityId { get; }
    VersionNumber EntityVersionNumber { get; }
}
