using System;

namespace EntityDb.Abstractions.Commands
{
    /// <summary>
    ///     Represents a command that has already been committed, along with relevant information not contained in the command.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity of the command.</typeparam>
    public interface IAnnotatedCommand<TEntity>
    {
        /// <summary>
        ///     The transaction id associated with the command.
        /// </summary>
        Guid TransactionId { get; }

        /// <summary>
        ///     The transaction timestamp associated with the command.
        /// </summary>
        DateTime TransactionTimeStamp { get; }

        /// <summary>
        ///     The entity id associated with the command.
        /// </summary>
        Guid EntityId { get; }

        /// <summary>
        ///     The entity version number associated with the command.
        /// </summary>
        ulong EntityVersionNumber { get; }

        /// <summary>
        ///     The command itself.
        /// </summary>
        ICommand<TEntity> Command { get; }
    }
}
