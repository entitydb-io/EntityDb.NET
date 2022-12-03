using EntityDb.DocumentationGenerator.Services.DocCommentService;
using EntityDb.DocumentationGenerator.Services.NodeService;
using EntityDb.DocumentationGenerator.Services.PrintingService;
using EntityDb.DocumentationGenerator.Services.TypeService;

ITypeService typeService = new TypeService();
INodeService nodeService = new NodeService();
IDocCommentService docCommentService = new DocCommentService();
IPrintingService printingService = new ConsolePrintingService();

var directory = new DirectoryInfo(AppContext.BaseDirectory);

var types = typeService.GetTypes(directory);
var namespaceNode = nodeService.GetNamespaceNode(types);

docCommentService.LoadInto(directory, namespaceNode);

printingService.Print(namespaceNode);