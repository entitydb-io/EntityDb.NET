using EntityDb.DocumentationGenerator.Services.AssemblyService;
using EntityDb.DocumentationGenerator.Services.DocCommentService;
using EntityDb.DocumentationGenerator.Services.NodeService;
using EntityDb.DocumentationGenerator.Services.PrintingService;

INodeService nodeService = new NodeService
(
    new AssemblyService(),
    new XmlDocCommentService()
);


var directory = new DirectoryInfo(AppContext.BaseDirectory);

var nodes = nodeService.Load(directory, "DocConfig.xml", "EntityDb.Abstractions.xml");

IPrintingService printingService = new ConsolePrintingService(nodes);

printingService.Print();