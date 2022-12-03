using System.Reflection;

namespace EntityDb.DocumentationGenerator.Helpers;

public static class MemberInfoHelper
{
    public static string GetXmlDocCommentName(MemberInfo memberInfo)
    {
        return memberInfo.Name.Replace(".", "#").Replace("&", "@");
    }
}
