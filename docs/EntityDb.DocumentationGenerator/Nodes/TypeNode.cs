namespace EntityDb.DocumentationGenerator.Nodes;

public class TypeNode : MemberInfoNode
{
    public Type Type { get; }

    public TypeNode(Type type) : base(type)
    {
        Type = type;
    }

    public override void AddChild(string path, Node node)
    {
        ChildNodes.Add(path, node);
    }
}