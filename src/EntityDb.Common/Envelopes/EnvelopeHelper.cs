using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Envelopes
{
    internal static class EnvelopeHelper
    {
        public const string Platform = nameof(Platform);
        public const string ThisPlatform = ".NET";
        public const string Type = nameof(Type);
        public const string AssemblyFullName = nameof(AssemblyFullName);
        public const string TypeFullName = nameof(TypeFullName);
        public const string MemberInfoName = nameof(MemberInfoName);

        public static bool NotThisPlatform(IReadOnlyDictionary<string, string> headers)
        {
            return headers.TryGetValue(Platform, out var platform) == false || platform != ThisPlatform;
        }

        public static bool TryGetAssemblyFullName(IReadOnlyDictionary<string, string> headers, out string? assemblyFullName)
        {
            return headers.TryGetValue(AssemblyFullName, out assemblyFullName);
        }

        public static bool TryGetTypeFullName(IReadOnlyDictionary<string, string> headers, out string? typeFullName)
        {
            return headers.TryGetValue(TypeFullName, out typeFullName);
        }

        public static bool TryGetMemberInfoName(IReadOnlyDictionary<string, string> headers, out string? memberInfoName)
        {
            return headers.TryGetValue(MemberInfoName, out memberInfoName);
        }

        public static Dictionary<string, string> GetTypeHeaders
        (
            Type type,
            bool includeFullNames = true,
            bool includeMemberInfoName = true
        )
        {
            var headers = new Dictionary<string, string>
            {
                [Platform] = ThisPlatform,
                [Type] = type.Name,
            };

            if (includeFullNames)
            {
                var assemblyFullName = type.Assembly.FullName;

                if (assemblyFullName != null)
                {
                    headers.Add(AssemblyFullName, assemblyFullName);
                }

                var typeFullName = type.FullName;

                if (typeFullName != null)
                {
                    headers.Add(TypeFullName, typeFullName);
                }
            }

            if (includeMemberInfoName)
            {
                headers.Add(MemberInfoName, type.Name);
            }

            return headers;
        }

        public static string[] GetTypeHeaderValues(this Type[] types)
        {
            return types.Select(type => type.Name).ToArray();
        }
    }
}
