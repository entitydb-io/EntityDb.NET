namespace EntityDb.DocumentationGenerator.Nodes;

public class TypeNode : Node
{
    public Type Type { get; }

    public TypeNode(Type type)
    {
        Type = type;
    }

    public override void Add(string path, Node node)
    {
        ChildNodes.Add(path, node);
    }
}