using System.Reflection;

namespace EntityDb.DocumentationGenerator.Nodes;

public class PropertyNode : MemberInfoNode
{
    public PropertyInfo PropertyInfo { get; }

    public PropertyNode(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo;
    }
}