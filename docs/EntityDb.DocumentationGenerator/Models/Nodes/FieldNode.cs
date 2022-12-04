using System.Reflection;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public class FieldNode : MemberInfoNode
{
    public FieldInfo FieldInfo { get; }

    public FieldNode(FieldInfo fieldInfo)
    {
        FieldInfo = fieldInfo;
    }
}
