using System.Reflection;

namespace EntityDb.DocumentationGenerator.Nodes;

public class FieldNode : MemberInfoNode
{
    public FieldInfo FieldInfo { get; }

    public FieldNode(FieldInfo fieldInfo)
    {
        FieldInfo = fieldInfo;
    }
}
