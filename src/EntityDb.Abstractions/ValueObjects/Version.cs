using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.States;

namespace EntityDb.Abstractions.ValueObjects;

/// <summary>
///     Represents a version for an state.
/// </summary>
/// <param name="Value">The backing value.</param>
public readonly record struct Version(ulong Value)
{
    /// <summary>
    ///     This constant represents the minimum possible version.
    ///     In the context of an <see cref="ISourceRepository" />,
    ///     this value is reserved for auto-increment behavior.
    ///     In the context of an <see cref="IStateRepository{TState}" />,
    ///     this value is reserved to point to the latest state.
    /// </summary>
    public static readonly Version Zero = new(ulong.MinValue);
    
    
    public static readonly Version One = new(1);

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
