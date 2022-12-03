using EntityDb.Abstractions.Tags;
using System.Collections.Immutable;

namespace EntityDb.Abstractions.Transactions.Steps;

/// <summary>
///     Represents a transaction step that adds tags.
/// </summary>
public interface IAddTagsTransactionStep : ITransactionStep
{
    /// <summary>
    ///     The tags that need to be added.
    /// </summary>
    ImmutableArray<ITag> Tags { get; }
}
