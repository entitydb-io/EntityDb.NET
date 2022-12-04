using EntityDb.DocumentationGenerator.Models.Nodes;

namespace EntityDb.DocumentationGenerator.Services.NodeService;

internal interface INodeService
{
    NamespaceNode GetNamespaceNode(DirectoryInfo directory);
}
