using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Abstractions.Sources.Annotations;

/// <summary>
///     Annotated source group data
/// </summary>
/// <typeparam name="TData">The type of the data</typeparam>
public interface IAnnotatedSourceGroupData<out TData>
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
    ///     The pointers to the entities
    /// </summary>
    Pointer[] EntityPointers { get; }
}
