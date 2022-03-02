using EntityDb.Abstractions.Tags;
using System.Collections.Immutable;

namespace EntityDb.Abstractions.Transactions.Steps;

/// <summary>
///     Represents a transaction step that deletes tags.
/// </summary>
public interface IDeleteTagsTransactionStep : ITransactionStep
{
    /// <summary>
    ///     The tags that need to be deleted.
    /// </summary>
    ImmutableArray<ITag> Tags { get; }
}
