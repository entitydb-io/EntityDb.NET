using System.Xml;
using System.Xml.Serialization;
using EntityDb.DocumentationGenerator.Models.Nodes;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

[XmlRoot("doc")]
public class DocFile
{
    [XmlElement("assembly")]
    public required AssemblyDoc Assembly { get; init; }

    [XmlArray("members")]
    [XmlArrayItem("member")]
    public required MemberDoc[] Members { get; init; }

    public void LoadInto(NamespaceNode namespaceNode)
    {
        var flatNodeDictionary = GetFlatNodeDictionary(namespaceNode);

        foreach (var member in Members)
        {
            var name = member.Name;

            if (flatNodeDictionary.TryGetValue(name, out var node))
            {
                foreach (var item in member.Items)
                {
                    node.AddDocumentation(item);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    private static Dictionary<string, Node> GetFlatNodeDictionary(NamespaceNode namespaceNode)
    {
        var flatNodeDictionary = new Dictionary<string, Node>();

        BuildFlatNodeDictionary(namespaceNode.GetChildNodes());

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
