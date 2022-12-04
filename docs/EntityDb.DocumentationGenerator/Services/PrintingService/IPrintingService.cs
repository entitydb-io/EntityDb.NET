using EntityDb.DocumentationGenerator.Models.Nodes;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Services.PrintingService;

public interface IPrintingService
{
    void Print(NamespaceNode namespaceNode);

    string ConvertSeeDoc(SeeDoc see);
    string ConvertParamRefDoc(ParamRefDoc paramRefDoc);
    string ConvertTypeParamRefDoc(TypeParamRefDoc typeParamRefDoc);
    string ConvertCodeDoc(CodeDoc codeDoc);
}
