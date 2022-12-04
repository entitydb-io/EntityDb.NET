using EntityDb.DocumentationGenerator.Models.Nodes;

namespace EntityDb.DocumentationGenerator.Services.NodeService;

internal interface INodeService
{
    Nodes Load(DirectoryInfo directory, params string[] docCommentFileNames);
}
