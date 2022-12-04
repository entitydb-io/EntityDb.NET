using System.Diagnostics.CodeAnalysis;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public class NestedTypesNode : INode
{
    public bool IsNamespace { get; }
    public int Count { get; private set; }
    public Dictionary<string, TypeNode> ClassNodes { get; init; } = new();
    public Dictionary<string, TypeNode> StructNodes { get; init; } = new();
    public Dictionary<string, TypeNode> InterfaceNodes { get; init; } = new();

    public NestedTypesNode(bool isNamespace)
    {
        IsNamespace = isNamespace;
    }

    public void AddChild(string path, TypeNode typeNode)
    {
        Count += 1;

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
    }

    public bool TryGetChild(string topPath, [NotNullWhen(true)] out TypeNode? typeNode)
    {
        if (ClassNodes.TryGetValue(topPath, out var classNode))
        {
            typeNode = classNode;
            return true;
        }

        if (StructNodes.TryGetValue(topPath, out var structNode))
        {
            typeNode = structNode;
            return true;
        }

        if (InterfaceNodes.TryGetValue(topPath, out var interfaceNode))
        {
            typeNode = interfaceNode;
            return true;
        }

        typeNode = null;
        return false;
    }

    public IEnumerable<KeyValuePair<string, INode>> GetChildNodes()
    {
        foreach (var childNode in GetChildNodesOf(ClassNodes))
        {
            yield return childNode;
        }

        foreach (var childNode in GetChildNodesOf(StructNodes))
        {
            yield return childNode;
        }

        foreach (var childNode in GetChildNodesOf(InterfaceNodes))
        {
            yield return childNode;
        }
    }

    private static IEnumerable<KeyValuePair<string, INode>> GetChildNodesOf(IDictionary<string, TypeNode> typeNodes)
    {
        foreach (var (path, node) in typeNodes)
        {
            yield return new(path, node);
        }
    }
}
