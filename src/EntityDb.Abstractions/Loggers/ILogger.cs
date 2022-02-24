using System;

namespace EntityDb.Abstractions.Loggers;

/// <summary>
///     Represents a type that logs errors (and possibly other things in the future?).
/// </summary>
public interface ILogger
{
    /// <summary>
    ///     Logs a caught <see cref="Exception" /> along with human-readable message about what was attempted before the
    ///     exception was thrown.
    /// </summary>
    /// <param name="exception">The exception that was caught.</param>
    /// <param name="message">
    ///     The human-readable message about what was attempted before <paramref name="exception" /> was
    ///     thrown.
    /// </param>
    void LogError(Exception exception, string message);
}
