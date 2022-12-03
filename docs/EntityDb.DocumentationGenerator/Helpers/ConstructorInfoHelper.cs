using System.Reflection;

namespace EntityDb.DocumentationGenerator.Helpers
{
    public static class ConstructorInfoHelper
    {
        public static string GetXmlDocCommentName(ConstructorInfo constructorInfo)
        {
            var parameterInfos = constructorInfo.GetParameters();

            return MemberInfoHelper.GetXmlDocCommentName(constructorInfo) + ParameterInfoHelper.GetXmlDocCommentNames(parameterInfos);
        }
    }
}
