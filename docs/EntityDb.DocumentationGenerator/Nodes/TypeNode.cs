namespace EntityDb.DocumentationGenerator.Nodes;

public class TypeNode : MemberInfoNode, INestableNode
{
    public Type Type { get; }
    public Dictionary<string, ConstructorNode> ConstructorNodes { get; init; } = new();
    public Dictionary<string, PropertyNode> PropertyNodes { get; init; } = new();
    public Dictionary<string, MethodNode> MethodNodes { get; init; } = new();

    public TypeNode(Type type)
    {
        Type = type;
    }

    public void AddChild(string path, Node node)
    {
        switch (node)
        {
            case ConstructorNode constructorNode:
                ConstructorNodes.Add(path, constructorNode);
                break;

            case PropertyNode propertyNode:
                PropertyNodes.Add(path, propertyNode);
                break;

            case MethodNode methodNode:
                MethodNodes.Add(path, methodNode);
                break;

            default:
                throw new NotImplementedException();
        }
    }

    public IEnumerable<KeyValuePair<string, Node>> GetChildNodes()
    {
        foreach (var (path, node) in ConstructorNodes)
        {
            yield return new (path, node);
        }

        foreach (var (path, node) in PropertyNodes)
        {
            yield return new (path, node);
        }

        foreach (var (path, node) in MethodNodes)
        {
            yield return new (path, node);
        }
    }
}