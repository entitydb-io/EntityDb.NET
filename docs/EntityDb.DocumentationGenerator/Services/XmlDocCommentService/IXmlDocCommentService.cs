using EntityDb.DocumentationGenerator.Models.Nodes;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Services.DocCommentService;

internal interface IXmlDocCommentService
{
    IEnumerable<DocFile> GetDocFiles(DirectoryInfo directoryInfo, string fileName);
    string GetNodeName(Node node);
}
