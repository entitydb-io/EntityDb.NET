using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public class NamespaceNode : Node, INestableNode
{
    public Dictionary<string, NamespaceNode> NamespaceNodes { get; init; } = new();
    public NestedTypesNode NestedTypesNode { get; init; } = new();

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

                NestedTypesNode.AddChild(path, typeNode);

                break;

            default:
                throw new NotImplementedException();
        }
    }

    public override void AddDocumentation(object docCommentMemberItem)
    {
        switch (docCommentMemberItem)
        {
            case SummaryDoc summaryDoc:
                SummaryDoc = summaryDoc;
                break;

            case RemarksDoc remarksDoc:
                RemarksDoc = remarksDoc;
                break;

            case InheritDoc inheritDoc:
                InheritDoc = inheritDoc;
                break;

            case IgnoreDoc:
                Ignore = true;
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

        foreach (var childNode in NestedTypesNode.GetChildNodes())
        {
            yield return childNode;
        }
    }

    private void DrillDownToAddChild(string path, Node node)
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

    public Dictionary<string, Node> ToXmlDocCommentMemberDictionary()
    {
        var flatNodeDictionary = new Dictionary<string, Node>();

        BuildFlatNodeDictionary(GetChildNodes());

        return flatNodeDictionary;

        void BuildFlatNodeDictionary(IEnumerable<KeyValuePair<string, Node>> nodes, string parentName = "")
        {
            foreach (var (name, node) in nodes)
            {
                var xmlDocCommentNamePrefix = node switch
                {
                    NamespaceNode => "N",
                    TypeNode => "T",
                    PropertyNode => "P",
                    MethodNode or ConstructorNode => "M",
                    FieldNode => "F",
                    _ => null
                };

                if (xmlDocCommentNamePrefix != null)
                {
                    flatNodeDictionary.Add($"{xmlDocCommentNamePrefix}:{parentName}{name}", node);
                }

                if (node is INestableNode nestableNode)
                {
                    BuildFlatNodeDictionary(nestableNode.GetChildNodes(), $"{parentName}{name}.");
                }
            }
        }
    }
}