namespace EntityDb.Abstractions.Disposables;

/// <summary>
///     Marks a resource as disposable (sync and async)
/// </summary>
public interface IDisposableResource : IDisposable, IAsyncDisposable
{
}
