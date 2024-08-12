using EntityDb.Abstractions.States;

namespace EntityDb.Abstractions.Sources.Annotations;

/// <summary>
///     Annotated message-level data
/// </summary>
/// <typeparam name="TData">The type of the data</typeparam>
public interface IAnnotatedMessageData<out TData>
{
    /// <summary>
    ///     The id of the source
    /// </summary>
    Id SourceId { get; }

    /// <summary>
    ///     The time stamp of the source
    /// </summary>
    TimeStamp SourceTimeStamp { get; }

    /// <summary>
    ///     The id of the message
    /// </summary>
    Id MessageId { get; }

    /// <summary>
    ///     The state pointer
    /// </summary>
    StatePointer StatePointer { get; }

    /// <summary>
    ///     The data
    /// </summary>
    TData Data { get; }
}
