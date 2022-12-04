using EntityDb.DocumentationGenerator.Models.XmlDocComment;
using EntityDb.DocumentationGenerator.Nodes;

namespace EntityDb.DocumentationGenerator.Services.PrintingService;

public interface IPrintingService
{
    void Print(NamespaceNode namespaceNode);

    string ConvertSeeDoc(SeeDoc see);
    string ConvertParamRefDoc(ParamRefDoc paramRefDoc);
    string ConvertTypeParamRefDoc(TypeParamRefDoc typeParamRefDoc);
    string ConvertCodeDoc(CodeDoc codeDoc);
}
