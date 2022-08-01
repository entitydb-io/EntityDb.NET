using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Documents;

internal interface IEntitiesDocument<TSerializedData> : ITransactionDocument<TSerializedData>
{
    Id[] EntityIds { get; }
}
