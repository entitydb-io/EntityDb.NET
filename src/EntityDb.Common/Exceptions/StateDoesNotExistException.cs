using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.States;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when <see cref="IEntityRepositoryFactory{TEntity}.CreateSingleForExisting" />,
///     <see cref="IMultipleEntityRepository{TEntity}.Load" />, or <see cref="IProjectionRepository{TProjection}.Get" />
///     cannot find the requested state.
/// </summary>
public sealed class StateDoesNotExistException : Exception
{
    private StateDoesNotExistException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Checks if the state pointer found satisfies the state pointer requested.
    /// </summary>
    /// <param name="requestedStatePointer">The state pointer requested by the agent.</param>
    /// <param name="foundStatePointer">The state pointer found by a query.</param>
    public static void ThrowIfNotAcceptable(StatePointer requestedStatePointer, StatePointer foundStatePointer)
    {
        if (requestedStatePointer.StateVersion == StateVersion.Zero &&
            foundStatePointer.StateVersion == StateVersion.Zero)
        {
            throw new StateDoesNotExistException("Requested latest but found none");
        }

        if (requestedStatePointer != foundStatePointer)
        {
            throw new StateDoesNotExistException($"Requested {requestedStatePointer} but found {foundStatePointer}.");
        }
    }
}
