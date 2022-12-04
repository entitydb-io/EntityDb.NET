using EntityDb.DocumentationGenerator.Models.Nodes;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Services.DocCommentService;

internal interface IXmlDocCommentService
{
    DocFile? GetDocFileOrDefault(DirectoryInfo directoryInfo, string fileName);
    string GetNodeName(Node node);
}
