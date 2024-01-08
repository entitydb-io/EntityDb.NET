namespace EntityDb.Abstractions.Sources;

/// <summary>
///     Represents a type that reacts to sources that have been committed.
/// </summary>
public interface ISourceSubscriber
{
    /// <summary>
    ///     Called when a source has been committed.
    /// </summary>
    /// <param name="source">The committed source.</param>
    void Notify(Source source);
}
