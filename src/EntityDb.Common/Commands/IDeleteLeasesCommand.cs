using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Transactions.Builders;
using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Commands;

/// <summary>
///     If a transaction needs to delete any instances of <see cref="ILease" />, and the properties of the leases
///     are contained in the command and/or entity, a direct call to
///     <see cref="ITransactionBuilder{TEntity}.Delete(Id, ILease[])" />
///     can be avoided by implementing this interface!
/// </summary>
/// <typeparam name="TEntity">The type of the entity</typeparam>
public interface IDeleteLeasesCommand<in TEntity>
{
    /// <summary>
    ///     Returns the leases that need to be deleted.
    /// </summary>
    /// <param name="previousEntity">The entity before this command was applied.</param>
    /// <param name="nextEntity">The entity after this command was applied.</param>
    /// <returns>The leases that need to be deleted.</returns>
    IEnumerable<ILease> GetLeases(TEntity previousEntity, TEntity nextEntity);
}
