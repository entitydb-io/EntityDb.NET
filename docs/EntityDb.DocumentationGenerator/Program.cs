using EntityDb.DocumentationGenerator.Services.AssemblyService;
using EntityDb.DocumentationGenerator.Services.DocCommentService;
using EntityDb.DocumentationGenerator.Services.NodeService;
using EntityDb.DocumentationGenerator.Services.PrintingService;
//using EntityDb.DocumentationGenerator.Services.PrintingService.ConsolePrinting;
using EntityDb.DocumentationGenerator.Services.PrintingService.MarkdownPrinting;

INodeService nodeService = new NodeService
(
    new AssemblyService(),
    new XmlDocCommentService()
);


var directory = new DirectoryInfo(AppContext.BaseDirectory);

var nodes = nodeService.Load(directory, "DocConfig.xml", "EntityDb.*.xml");

var directoryPath = @"C:\Users\theav\Documents\GitHub\entitydb-io.github.io\dotnet";

if (!Directory.Exists(directoryPath))
{
    Directory.CreateDirectory(directoryPath);
}

var writeDirectory = new DirectoryInfo(directoryPath);

//IPrintingService printingService = new ConsolePrintingService(nodes);
IPrintingService printingService = new NamespaceGitHubPrintingService(nodes, "/dotnet", writeDirectory);

printingService.Print();