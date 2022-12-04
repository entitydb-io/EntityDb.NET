using EntityDb.DocumentationGenerator.Services.AssemblyService;
using EntityDb.DocumentationGenerator.Services.DocCommentService;
using EntityDb.DocumentationGenerator.Services.NodeService;
using EntityDb.DocumentationGenerator.Services.PrintingService;
using EntityDb.DocumentationGenerator.Services.PrintingService.ConsolePrinting;

INodeService nodeService = new NodeService
(
    new AssemblyService(),
    new XmlDocCommentService()
);


var directory = new DirectoryInfo(AppContext.BaseDirectory);

var nodes = nodeService.Load(directory, "EntityDb.Common.xml");

IPrintingService printingService = new ConsolePrintingService(nodes);

printingService.Print();