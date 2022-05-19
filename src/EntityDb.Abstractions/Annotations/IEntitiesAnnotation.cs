using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Annotations;

/// <summary>
///     Represents data for multiple entities that have already been committed, along with relevant information not contained
///     in the data.
/// </summary>
/// <typeparam name="TData">The type of data.</typeparam>
public interface IEntitiesAnnotation<out TData>
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
    ///     The entity ids associated with the data.
    /// </summary>
    Id[] EntityIds { get; }

    /// <summary>
    ///     The data.
    /// </summary>
    TData Data { get; }
}
