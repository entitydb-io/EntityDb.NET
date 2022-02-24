using EntityDb.Abstractions.Commands;

namespace EntityDb.Abstractions.Transactions.Steps;

/// <summary>
///     Represents a modification to an entity's state.
/// </summary>
/// <typeparam name="TEntity">The type of entity to be modified.</typeparam>
public interface ICommandTransactionStep<TEntity> : ITransactionStep<TEntity>
{
    /// <summary>
    ///     The modifier.
    /// </summary>
    ICommand<TEntity> Command { get; }

    /// <summary>
    ///     A snapshot of the entity before the command.
    /// </summary>
    TEntity PreviousEntitySnapshot { get; }

    /// <summary>
    ///     The previous version number of the entity.
    /// </summary>
    ulong PreviousEntityVersionNumber { get; }

    /// <summary>
    ///     A snapshot of the entity after the command.
    /// </summary>
    TEntity NextEntitySnapshot { get; }

    /// <summary>
    ///     The next version number of the entity.
    /// </summary>
    ulong NextEntityVersionNumber { get; }
}
