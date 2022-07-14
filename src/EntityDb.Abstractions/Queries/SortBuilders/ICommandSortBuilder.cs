using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace EntityDb.Abstractions.Queries.SortBuilders;

/// <summary>
///     Builds a <typeparamref name="TSort" /> for a command query.
/// </summary>
/// <typeparam name="TSort">The type of sort used by the repository.</typeparam>
public interface ICommandSortBuilder<TSort> : ISortBuilder<TSort>
{
    /// <ignore/>
    [Obsolete("This method will be removed in the future, and may not be supported for all implementations.")]
    [ExcludeFromCodeCoverage(Justification = "Obsolete")]
    TSort CommandProperty<TCommand>(bool ascending, Expression<Func<TCommand, object>> commandExpression)
        => throw new NotSupportedException();

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders commands by entity id.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders commands by entity id.</returns>
    TSort EntityId(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders commands by entity version number.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders commands by entity version number.</returns>
    TSort EntityVersionNumber(bool ascending);

    /// <summary>
    ///     Returns a <typeparamref name="TSort" /> that orders commands by type.
    /// </summary>
    /// <param name="ascending">Pass <c>true</c> for ascending order or <c>false</c> for descending order.</param>
    /// <returns>A <typeparamref name="TSort" /> that orders commands by type.</returns>
    TSort CommandType(bool ascending);
}
