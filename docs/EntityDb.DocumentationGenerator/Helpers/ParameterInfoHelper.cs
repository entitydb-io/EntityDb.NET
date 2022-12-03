using System.Collections.Generic;
using System.Reflection;

namespace EntityDb.DocumentationGenerator.Helpers
{
    public static class ParameterInfoHelper
    {
        public static string GetXmlDocCommentNames(ParameterInfo[] parameterInfos)
        {
            if (parameterInfos.Length == 0)
            {
                return "";
            }

            var list = new List<string>();

            foreach (var parameterInfo in parameterInfos)
            {
                list.Add(TypeHelper.GetXmlDocCommentName(parameterInfo.ParameterType));
            }

            return $"({string.Join(",", list)})";
        }
    }
}
