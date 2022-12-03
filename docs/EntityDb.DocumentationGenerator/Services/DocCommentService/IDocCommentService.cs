using EntityDb.DocumentationGenerator.Nodes;

namespace EntityDb.DocumentationGenerator.Services.DocCommentService;

internal interface IDocCommentService
{
    void LoadInto(DirectoryInfo directory, NamespaceNode namespaceNode);
}
