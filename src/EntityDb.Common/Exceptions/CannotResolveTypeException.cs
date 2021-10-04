using EntityDb.Common.Strategies.Resolving;
using System;

namespace EntityDb.Common.Exceptions
{
    /// <summary>
    ///     The exception that is thrown when the <see cref="LifoResolvingStrategyChain" /> cannot resolve a type.
    /// </summary>
    public sealed class CannotResolveTypeException : Exception
    {
    }
}
