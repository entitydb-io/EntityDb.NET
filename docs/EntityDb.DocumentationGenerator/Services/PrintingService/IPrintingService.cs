using EntityDb.DocumentationGenerator.Nodes;

namespace EntityDb.DocumentationGenerator.Services.PrintingService;

internal interface IPrintingService
{
    void Print(NamespaceNode namespaceNode);
}
