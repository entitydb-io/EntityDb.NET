using EntityDb.DocumentationGenerator.Services.AssemblyService;
using EntityDb.DocumentationGenerator.Services.DocCommentService;
using EntityDb.DocumentationGenerator.Services.NodeService;
using EntityDb.DocumentationGenerator.Services.PrintingService;

INodeService nodeService = new NodeService
(
    new AssemblyService(),
    new DocCommentService()
);

IPrintingService printingService = new ConsolePrintingService();

var directory = new DirectoryInfo(AppContext.BaseDirectory);

var namespaceNode = nodeService.GetNamespaceNode(directory);

printingService.Print(namespaceNode);