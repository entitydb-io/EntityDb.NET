using EntityDb.Common.Snapshots;
using System.Linq.Expressions;

namespace EntityDb.EntityFramework.Snapshots;

/// <summary>
///     Indicates that a snapshot is compatible with EntityDb.EntityFramework implementations.
/// </summary>
/// <typeparam name="TSnapshot">The type of the snapshot</typeparam>
public interface IEntityFrameworkSnapshot<TSnapshot> : ISnapshot<TSnapshot>
{
    /// <summary>
    ///     Returns an expression of a function that can be used to check if a copy of this record already exists.
    /// </summary>
    /// <returns>An expression of a function that can be used to check if a copy of this record already exists.</returns>
    Expression<Func<TSnapshot, bool>> GetKeyPredicate();
}
