using EntityDb.Abstractions.Transactions;

namespace EntityDb.Abstractions.ValueObjects;

/// <summary>
///     Represents a particular version for an object.
/// </summary>
/// <param name="Value">The backing value.</param>
public readonly record struct VersionNumber(ulong Value)
{
    /// <summary>
    ///     This constant represents the minimum possible version number.
    ///     In the context of an <see cref="ITransactionRepository" />,
    ///     this value is reserved to indicate there is no previous version number.
    ///     In the context of an <see cref="Pointer" />,
    ///     this value is reserved to point to the latest snapshot.
    /// </summary>
    public static readonly VersionNumber MinValue = new(ulong.MinValue);

    /// <summary>
    ///     Gets the next version number.
    /// </summary>
    /// <returns>The next version number.</returns>
    public VersionNumber Next()
    {
        return new VersionNumber(Value + 1);
    }

    /// <summary>
    ///     Gets the previous version number.
    /// </summary>
    /// <returns>The previous version number.</returns>
    public VersionNumber Previous()
    {
        return new VersionNumber(Value - 1);
    }

    /// <summary>
    ///     Converts the numeric value of this instance to its equivalent string
    ///     representation.
    /// </summary>
    /// <returns>
    ///     The string representation of the value of this instance, consisting
    ///     of a sequence of digits ranging from 0 to 9, without a sign or
    ///     leading zeroes.
    /// </returns>
    public override string ToString()
    {
        return Value.ToString();
    }
}
