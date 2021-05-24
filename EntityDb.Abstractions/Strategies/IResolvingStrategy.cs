using System;
using System.Reflection;

namespace EntityDb.Abstractions.Strategies
{
    /// <summary>
    /// Represents a type that resolves a <see cref="Type"/> by using the <see cref="Assembly.FullName"/>, <see cref="Type.FullName"/>, and <see cref="MemberInfo.Name"/>.
    /// </summary>
    public interface IResolvingStrategy
    {
        /// <summary>
        /// Returns the resolved <see cref="Type"/> or null if the <see cref="Type"/> cannot be resolved.
        /// </summary>
        /// <param name="assemblyFullName">The <see cref="Assembly.FullName"/> of the <see cref="Type.Assembly"/>.</param>
        /// <param name="typeFullName">The <see cref="Type.FullName"/>.</param>
        /// <param name="typeName">The <see cref="MemberInfo.Name"/>.</param>
        /// <returns>The resolved <see cref="Type"/> or null if the <see cref="Type"/> cannot be resolved.</returns>
        Type? ResolveType(string? assemblyFullName, string? typeFullName, string? typeName);
    }
}
