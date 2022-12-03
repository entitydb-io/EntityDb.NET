using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.DocumentationGenerator.Helpers
{
    public static class TypeHelper
    {
        public static string GetXmlDocCommentName(Type type)
        {
            if (type.IsArray)
            {
                var elementName = GetXmlDocCommentName(type.GetElementType()!);

                var arrayIndicator = $"[{string.Join(",", Enumerable.Repeat("", type.GetArrayRank()))}]";

                return $"{elementName}{arrayIndicator}";
            }


            if (type.IsGenericTypeParameter)
            {
                return $"`{type.GenericParameterPosition}";
            }

            if (type.IsGenericMethodParameter)
            {
                return $"``{type.GenericParameterPosition}";
            }

            if (type.IsGenericType)
            {
                var list = new List<string>();

                foreach (var genericArgument in type.GetGenericArguments())
                {
                    list.Add(GetXmlDocCommentName(genericArgument));
                }

                var baseName = type.Name[..type.Name.IndexOf('`')];

                return $"{type.Namespace}.{baseName}{{{string.Join(',', list)}}}";
            }

            return $"{type.Namespace}.{MemberInfoHelper.GetXmlDocCommentName(type)}";
        }
    }
}
