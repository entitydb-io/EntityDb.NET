using EntityDb.Abstractions.States;

namespace EntityDb.Abstractions.Sources.Annotations;

/// <summary>
///     Annotated source-level data
/// </summary>
/// <typeparam name="TData">The type of the data</typeparam>
public interface IAnnotatedSourceData<out TData>
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
    ///     The message ids
    /// </summary>
    Id[] MessageIds { get; }

    /// <summary>
    ///     The state pointers
    /// </summary>
    StatePointer[] StatePointers { get; }

    /// <summary>
    ///     The data
    /// </summary>
    TData Data { get; }
}
