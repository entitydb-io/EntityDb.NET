using System.Reflection;
using EntityDb.DocumentationGenerator.Helpers;

namespace EntityDb.DocumentationGenerator.Nodes;

public class PropertyNode : MemberInfoNode
{
    public PropertyInfo PropertyInfo { get; }

    public PropertyNode(PropertyInfo propertyInfo) : base(propertyInfo)
    {
        PropertyInfo = propertyInfo;
    }

    public override string GetXmlDocCommentName()
    {
        return PropertyInfoHelper.GetXmlDocCommentName(PropertyInfo);
    }
}