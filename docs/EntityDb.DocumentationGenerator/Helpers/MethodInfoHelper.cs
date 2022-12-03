using System.Reflection;

namespace EntityDb.DocumentationGenerator.Helpers;

public static class MethodInfoHelper
{
    public static string GetXmlDocCommentName(MethodInfo methodInfo)
    {
        var parameterInfos = methodInfo.GetParameters();

        var genericIndicator = string.Empty;

        if (methodInfo.IsGenericMethod)
        {
            genericIndicator = $"``{methodInfo.GetGenericArguments().Length}";
        }

        return MemberInfoHelper.GetXmlDocCommentName(methodInfo) + genericIndicator + ParameterInfoHelper.GetXmlDocCommentNames(parameterInfos);
    }
}
