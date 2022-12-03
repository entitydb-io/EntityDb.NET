using System.Xml.XPath;
using System.Xml;
using EntityDb.DocumentationGenerator.Nodes;

namespace EntityDb.DocumentationGenerator.Services.DocCommentService;

public class DocCommentService : IDocCommentService
{
    public void LoadInto(DirectoryInfo directory, NamespaceNode namespaceNode)
    {
        var documentationFiles = directory.GetFiles("EntityDb.*.xml")
            .Select(documentationFile =>
            {
                var document = new XmlDocument();

                using var xmlFile = documentationFile.OpenRead();

                return new XPathDocument(xmlFile).CreateNavigator();
            });

        var flatNodeDictionary = GetFlatNodeDictionary(namespaceNode);

        foreach (var documentationFile in documentationFiles)
        {
            var members = documentationFile.Select("doc/members/member");

            foreach (XPathNavigator member in members)
            {
                var name = member.GetAttribute("name", "");

                if (flatNodeDictionary.TryGetValue(name, out var node))
                {
                    foreach (XPathNavigator child in member.Select("*"))
                    {
                        node.AddDocumentation(child);
                    }
                }
            }
        }
    }

    private static Dictionary<string, Node> GetFlatNodeDictionary(NamespaceNode namespaceNode)
    {
        var flatNodeDictionary = new Dictionary<string, Node>();

        BuildFlatNodeDictionary(namespaceNode.GetAllChildren());

        return flatNodeDictionary;

        void BuildFlatNodeDictionary(IEnumerable<KeyValuePair<string, Node>> childNodes, string parentName = "")
        {
            foreach (var (name, node) in childNodes)
            {
                var xmlDocCommentNamePrefix = node switch
                {
                    TypeNode => "T",
                    PropertyNode => "P",
                    MethodNode or ConstructorNode => "M",
                    _ => null
                };

                if (xmlDocCommentNamePrefix != null)
                {
                    flatNodeDictionary.Add($"{xmlDocCommentNamePrefix}:{parentName}{name}", node);
                }

                if (node is INestableNode nestableNode)
                {
                    BuildFlatNodeDictionary(nestableNode.GetAllChildren(), $"{parentName}{name}.");
                }
            }
        }
    }
}