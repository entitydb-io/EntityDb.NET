using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Annotations;

/// <summary>
///     Represents data for a single entity that has already been committed, along with relevant information not contained
///     in the data.
/// </summary>
/// <typeparam name="TData">The type of data.</typeparam>
public interface IEntityAnnotation<out TData>
{
    /// <summary>
    ///     The transaction id associated with the data.
    /// </summary>
    Id TransactionId { get; }

    /// <summary>
    ///     The transaction timestamp associated with the data.
    /// </summary>
    TimeStamp TransactionTimeStamp { get; }

    /// <summary>
    ///     The entity id associated with the data.
    /// </summary>
    Id EntityId { get; }

    /// <summary>
    ///     The entity version number associated with the data.
    /// </summary>
    VersionNumber EntityVersionNumber { get; }

    /// <summary>
    ///     The data.
    /// </summary>
    TData Data { get; }
}
