namespace EntityDb.DocumentationGenerator.Models.Nodes;

public class Nodes
{
    public required IDictionary<string, Node> XmlDocCommentMemberDictionary { get; init; }

    public required NamespaceNode Root { get; init; }
}
