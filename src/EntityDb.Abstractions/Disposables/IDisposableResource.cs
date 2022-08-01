namespace EntityDb.Abstractions.Disposables;

/// <summary>
///     Marks a resource as disposable and provides a default implementation.
/// </summary>
public interface IDisposableResource : IDisposable, IAsyncDisposable
{
}
