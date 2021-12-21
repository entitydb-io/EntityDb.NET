using System;

namespace EntityDb.Common.Exceptions
{
    /// <summary>
    ///     The exception that is thrown if a test mode condition is not met.
    /// </summary>
    public sealed class TestModeException : Exception
    {
        /// <inheritdoc/>
        public TestModeException(string message) : base(message)
        {
        }
    }
}
