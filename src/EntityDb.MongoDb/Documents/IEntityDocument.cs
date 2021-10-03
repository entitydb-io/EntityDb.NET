using System;

namespace EntityDb.MongoDb.Documents
{
    internal interface IEntityDocument : ITransactionDocument
    {
        Guid EntityId { get; }
        ulong EntityVersionNumber { get; }
    }
}
