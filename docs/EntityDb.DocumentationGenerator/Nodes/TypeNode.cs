namespace EntityDb.DocumentationGenerator.Nodes;

public class TypeNode : MemberInfoNode, INestableNode
{
    public Type Type { get; }
    public Dictionary<string, TypeNode> NestedTypeNodes { get; init; } = new();
    public Dictionary<string, FieldNode> FieldNodes { get; init; } = new();
    public Dictionary<string, ConstructorNode> ConstructorNodes { get; init; } = new();
    public Dictionary<string, PropertyNode> PropertyNodes { get; init; } = new();
    public Dictionary<string, MethodNode> MethodNodes { get; init; } = new();

    public TypeNode(Type type)
    {
        Type = type;
    }

    public void AddChild(string path, INode node)
    {
        switch (node)
        {
            case TypeNode typeNode:
                NestedTypeNodes.Add(path, typeNode);
                break;

            case FieldNode fieldNode:
                FieldNodes.Add(path, fieldNode);
                break;

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

    public IEnumerable<KeyValuePair<string, INode>> GetChildNodes()
    {
        foreach (var (path, node) in NestedTypeNodes)
        {
            yield return new(path, node);

            foreach (var nestedChildNode in node.GetChildNodes())
            {
                yield return nestedChildNode;
            }
        }

        foreach (var (path, node) in FieldNodes)
        {
            yield return new(path, node);
        }

        foreach (var (path, node) in ConstructorNodes)
        {
            yield return new(path, node);
        }

        foreach (var (path, node) in PropertyNodes)
        {
            yield return new(path, node);
        }

        foreach (var (path, node) in MethodNodes)
        {
            yield return new(path, node);
        }
    }
}