using System.Reflection;

namespace EntityDb.DocumentationGenerator.Helpers
{
    public static class PropertyInfoHelper
    {
        public static string GetXmlDocCommentName(PropertyInfo propertyInfo)
        {
            var indexParameterInfos = propertyInfo.GetIndexParameters();

            if (indexParameterInfos?.Length > 0)
            {
                return MemberInfoHelper.GetXmlDocCommentName(propertyInfo) + ParameterInfoHelper.GetXmlDocCommentNames(indexParameterInfos);
            }

            return MemberInfoHelper.GetXmlDocCommentName(propertyInfo);
        }
    }
}
