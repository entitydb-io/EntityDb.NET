using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Sources.Annotations;

/// <summary>
///     Annotated source data
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
    ///     The data
    /// </summary>
    TData Data { get; }

    /// <summary>
    ///     A pointer to the entity
    /// </summary>
    Pointer EntityPointer { get; }
}
