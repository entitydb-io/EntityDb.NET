using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Services.PrintingService;

public interface IPrintingService
{
    void Print();
    string ConvertSeeDoc(SeeDoc seeDoc);
    string ConvertParamRefDoc(ParamRefDoc paramRefDoc);
    string ConvertTypeParamRefDoc(TypeParamRefDoc typeParamRefDoc);
    string ConvertCodeDoc(CodeDoc codeDoc);
}
