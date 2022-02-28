using System;

namespace EntityDb.Abstractions.Statements;

/// <summary>
///     Represents information that can be applied to a <see cref="TProjection"/>.
/// </summary>
/// <typeparam name="TProjection">The type of the projection.</typeparam>
public interface IStatement<TProjection>
{
    /// <summary>
    ///     Returns a new <typeparamref name="TProjection" /> that incorporates the modification into an entity.
    /// </summary>
    /// <param name="entityId">The id of the entity for which the statement is made.</param>
    /// <param name="projection">The projection to which the statement is applied.</param>
    /// <returns>
    ///     A new <typeparamref name="TProjection" /> that incorporates the modification into an entity.
    /// </returns>
    TProjection Reduce(Guid entityId, TProjection projection);
}
