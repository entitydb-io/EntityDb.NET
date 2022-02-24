using System;

namespace EntityDb.MongoDb.Documents;

internal interface IEntitiesDocument : ITransactionDocument
{
    Guid[] EntityIds { get; }
}
