using System.Reflection;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public class PropertyNode : MemberInfoNode
{
    public PropertyInfo PropertyInfo { get; }

    public PropertyNode(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo;
    }
}