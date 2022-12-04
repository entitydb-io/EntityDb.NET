namespace EntityDb.DocumentationGenerator.Nodes;

public class NamespaceNode : INode, INestableNode
{
    public Dictionary<string, NamespaceNode> NamespaceNodes { get; init; } = new();
    public NestedTypesNode NestedTypesNode { get; init; } = new(true);

    public void AddChild(string path, INode node)
    {
        switch (node)
        {
            case TypeNode typeNode:
                if (path.Contains('.'))
                {
                    DrillDownToAddChild(path, node);
                    return;
                }

                NestedTypesNode.AddChild(path, typeNode);

                break;

            default:
                throw new NotImplementedException();
        }
    }

    public IEnumerable<KeyValuePair<string, INode>> GetChildNodes()
    {
        foreach (var (path, node) in NamespaceNodes)
        {
            yield return new(path, node);
        }

        foreach (var childNode in NestedTypesNode.GetChildNodes())
        {
            yield return childNode;
        }
    }

    private void DrillDownToAddChild(string path, INode node)
    {
        var pathComponents = path.Split('.', 2);

        var topPath = pathComponents.ElementAt(0);
        var subPath = pathComponents.ElementAtOrDefault(1) ?? "";

        if (NestedTypesNode.TryGetChild(topPath, out var typeNode))
        {
            typeNode.AddChild(subPath, node);
        }

        if (!NamespaceNodes.TryGetValue(topPath, out var namespaceNode))
        {
            namespaceNode = new NamespaceNode();

            NamespaceNodes.Add(topPath, namespaceNode);
        }

        namespaceNode.AddChild(subPath, node);
    }
}