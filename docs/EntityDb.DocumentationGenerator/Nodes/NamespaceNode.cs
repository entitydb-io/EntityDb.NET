namespace EntityDb.DocumentationGenerator.Nodes
{
    public class NamespaceNode : Node
    {
        public override void Add(string path, Node node)
        {
            var components = path.Split('.', 2);

            if (!path.Contains('.'))
            {
                ChildNodes.Add(path, node);
                return;
            }

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