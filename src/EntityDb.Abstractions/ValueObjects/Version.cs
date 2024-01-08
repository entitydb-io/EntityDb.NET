using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Sources;

namespace EntityDb.Abstractions.ValueObjects;

/// <summary>
///     Represents a version for an entity or projection.
/// </summary>
/// <param name="Value">The backing value.</param>
public readonly record struct Version(ulong Value)
{
    /// <summary>
    ///     This constant represents the minimum possible version.
    ///     In the context of an <see cref="ISourceRepository" />,
    ///     this value is reserved for auto-increment behavior.
    ///     In the context of an <see cref="ISnapshotRepository{TSnapshot}" />,
    ///     this value is reserved to point to the latest snapshot.
    /// </summary>
    public static readonly Version Zero = new(ulong.MinValue);

    /// <summary>
    ///     Returns the next version.
    /// </summary>
    /// <returns>The next version.</returns>
    public Version Next()
    {
        return new Version(Value + 1);
    }

    /// <inheritdoc cref="ulong.ToString()" />
    public override string ToString()
    {
        return Value.ToString();
    }
}
