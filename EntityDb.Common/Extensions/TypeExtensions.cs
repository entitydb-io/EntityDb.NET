using System;
using System.Linq;

namespace EntityDb.Common.Extensions
{
    internal static class TypeExtensions
    {
        public static (string? assemblyFullName, string? typeFullName, string? typeName) GetTypeInfo(this Type type)
        {
            return (type.Assembly.FullName, type.FullName, type.Name);
        }

        public static string[] GetTypeNames(this Type[] types)
        {
            return types.Select(type => type.Name).ToArray();
        }
    }
}
