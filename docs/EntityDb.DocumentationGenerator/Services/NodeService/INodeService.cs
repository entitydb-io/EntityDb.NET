using EntityDb.DocumentationGenerator.Nodes;

namespace EntityDb.DocumentationGenerator.Services.NodeService;

internal interface INodeService
{
    NamespaceNode GetNamespaceNode(IEnumerable<Type> types);
}
