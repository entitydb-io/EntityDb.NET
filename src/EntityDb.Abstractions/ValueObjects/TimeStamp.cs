using System;
using System.Globalization;

namespace EntityDb.Abstractions.ValueObjects;

/// <summary>
///     Represents a relevant moment in time.
/// </summary>
/// <param name="Value">The backing value.</param>
public readonly record struct TimeStamp(DateTime Value)
{
    /// <summary>
    ///     The value of this constant is equivalent to 00:00:00.0000000 UTC, January 1, 1970.
    /// </summary>
    public static readonly TimeStamp UnixEpoch = new(DateTime.UnixEpoch);

    /// <summary>
    ///     Gets a <see cref="TimeStamp"/> that represents the current date and time on this computer, expressed in UTC.
    /// </summary>
    public static TimeStamp UtcNow => new(DateTime.UtcNow);

    /// <summary>
    ///     Gets a <see cref="TimeStamp"/> rounded down to the nearest millisecond.
    /// </summary>
    /// <returns>A <see cref="TimeStamp"/> rounded down to the nearest millisecond.</returns>
    public TimeStamp WithMillisecondPrecision() =>
        new(Value - TimeSpan.FromTicks(Value.Ticks % TimeSpan.TicksPerMillisecond));

    /// <summary>
    ///     Converts the value of the current <see cref="TimeStamp"/> object to
    ///     its equivalent string representation using the formatting
    ///     conventions of the current culture.
    /// </summary>
    /// <returns>
    ///     A string representation of the value of the current
    ///     <see cref="TimeStamp"/> object.
    /// </returns>
    public override string? ToString() => Value.ToString(CultureInfo.CurrentCulture);
}
