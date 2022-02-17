using System.Collections.Generic;

public abstract record Node()
{
    public Dictionary<string, Node> ChildNodes { get; init; } = new();

    public virtual void Add(string path, Node node)
    {
        var components = path.Split('.', 2);

        if (components.Length == 1)
        {
            ChildNodes.Add(path, node);
        }
        else
        {
            var highestComponent = components[0];

            if (!ChildNodes.TryGetValue(highestComponent, out var childNode))
            {
                childNode = new NamespaceNode();

                ChildNodes.Add(highestComponent, childNode);
            }

            childNode.Add(components[1], node);

        }
    }
}

public record NamespaceNode : Node
{
}