using System.Reflection;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public class PropertyNode : Node
{
    public PropertyInfo PropertyInfo { get; }

    public PropertyNode(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo;
    }
}