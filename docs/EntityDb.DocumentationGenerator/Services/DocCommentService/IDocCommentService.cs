using EntityDb.DocumentationGenerator.Nodes;

namespace EntityDb.DocumentationGenerator.Services.DocCommentService;

internal interface IDocCommentService
{
    string GetNodeName(INode node);
    void LoadInto(DirectoryInfo directory, NamespaceNode namespaceNode);
}
