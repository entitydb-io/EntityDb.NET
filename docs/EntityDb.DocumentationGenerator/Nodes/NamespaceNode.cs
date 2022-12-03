namespace EntityDb.DocumentationGenerator.Nodes;

public class NamespaceNode : Node, INestableNode
{
    public Dictionary<string, NamespaceNode> NamespaceNodes { get; init; } = new();
    public Dictionary<string, TypeNode> ClassNodes { get; init; } = new();
    public Dictionary<string, TypeNode> StructNodes { get; init; } = new();
    public Dictionary<string, TypeNode> InterfaceNodes { get; init; } = new();
    public int TypeNodeCount { get; set; }

    public void AddChild(string path, Node node)
    {
        switch (node)
        {
            case TypeNode typeNode:
                if (path.Contains('.'))
                {
                    DrillDownToAddChild(path, node);
                    return;
                }

                TypeNodeCount += 1;

                if (typeNode.Type.IsClass)
                {
                    ClassNodes.Add(path, typeNode);
                }
                else if (typeNode.Type.IsValueType)
                {
                    StructNodes.Add(path, typeNode);
                }
                else if (typeNode.Type.IsInterface)
                {
                    InterfaceNodes.Add(path, typeNode);
                }
                else
                {
                    throw new NotImplementedException();
                }

                break;

            default:
                throw new NotImplementedException();
        }
    }

    public IEnumerable<KeyValuePair<string, Node>> GetChildNodes()
    {
        foreach (var (path, node) in NamespaceNodes)
        {
            yield return new(path, node);
        }

        foreach (var (path, node) in ClassNodes)
        {
            yield return new(path, node);
        }

        foreach (var (path, node) in StructNodes)
        {
            yield return new(path, node);
        }

        foreach (var (path, node) in InterfaceNodes)
        {
            yield return new(path, node);
        }
    }

    private void DrillDownToAddChild(string path, Node node)
    {
        var pathComponents = path.Split('.', 2);

        var topPath = pathComponents.ElementAt(0);
        var subPath = pathComponents.ElementAtOrDefault(1) ?? "";

        if (ClassNodes.TryGetValue(topPath, out var classNode))
        {
            classNode.AddChild(subPath, node);
        }

        if (StructNodes.TryGetValue(topPath, out var structNode))
        {
            structNode.AddChild(subPath, node);
        }

        if (InterfaceNodes.TryGetValue(topPath, out var interfaceNode))
        {
            interfaceNode.AddChild(subPath, node);
        }

        if (!NamespaceNodes.TryGetValue(topPath, out var namespaceNode))
        {
            namespaceNode = new NamespaceNode();

            NamespaceNodes.Add(topPath, namespaceNode);
        }

        namespaceNode.AddChild(subPath, node);
    }
}