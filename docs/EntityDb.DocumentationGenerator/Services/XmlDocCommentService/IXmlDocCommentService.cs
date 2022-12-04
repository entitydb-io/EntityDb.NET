using EntityDb.DocumentationGenerator.Models.Nodes;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Services.DocCommentService;

internal interface IXmlDocCommentService
{
    DocFile GetDocFile(DirectoryInfo directoryInfo, string fileName);
    string GetNodeName(Node node);
}
