using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Documents;

internal interface IEntitiesDocument<out TSerializedData> : ITransactionDocument<TSerializedData>
{
    Id[] EntityIds { get; }
}
