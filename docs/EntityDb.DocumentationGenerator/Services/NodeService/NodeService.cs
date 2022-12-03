using System.Reflection;
using System.Runtime.CompilerServices;
using EntityDb.DocumentationGenerator.Nodes;
using EntityDb.DocumentationGenerator.Services.AssemblyService;
using EntityDb.DocumentationGenerator.Services.DocCommentService;

namespace EntityDb.DocumentationGenerator.Services.NodeService;

internal class NodeService : INodeService
{
    private readonly IAssemblyService _assemblyService;
    private readonly IDocCommentService _docCommentService;

    public NodeService(IAssemblyService assemblyService, IDocCommentService docCommentService)
    {
        _assemblyService = assemblyService;
        _docCommentService = docCommentService;
    }

    public NamespaceNode GetNamespaceNode(DirectoryInfo directory)
    {
        var namespaceNode = new NamespaceNode();

        var assemblies = _assemblyService.GetAssemblies(directory);

        foreach (var assembly in assemblies)
        {
            namespaceNode.AddChild(assembly.GetName().Name!, new AssemblyNode(assembly));

            var types = assembly.GetTypes()
                .Where(type => type.IsPublic)
                .OrderBy(type => type.Namespace);

            foreach (var type in types)
            {
                var typeNode = new TypeNode(type);

                namespaceNode.AddChild(type.FullName!, typeNode);

                var bindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

                var constructorInfos = type
                    .GetConstructors(bindingFlags)
                    .Where(constructorInfo => constructorInfo.GetCustomAttribute<CompilerGeneratedAttribute>() == null);

                var propertyInfos = type
                    .GetProperties(bindingFlags)
                    .Where(propertyInfo => propertyInfo.GetCustomAttribute<CompilerGeneratedAttribute>() == null);

                var methodInfos = type
                    .GetMethods(bindingFlags)
                    .Where(methodInfo => methodInfo.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
                    .Except(propertyInfos
                        .SelectMany(propertyInfo => propertyInfo.GetAccessors()));

                foreach (var constructorInfo in constructorInfos)
                {
                    if (constructorInfo.IsPrivate || constructorInfo.IsAssembly)
                    {
                        continue;
                    }

                    var node = new ConstructorNode(constructorInfo);
                    var nodeDocCommentName = _docCommentService.GetNodeName(node);

                    typeNode.AddChild(nodeDocCommentName, node);
                }

                foreach (var propertyInfo in propertyInfos)
                {
                    var accessors = propertyInfo.GetAccessors();

                    if (accessors.All(methodInfo => methodInfo.IsPrivate || methodInfo.IsAssembly))
                    {
                        continue;
                    }

                    var node = new PropertyNode(propertyInfo);
                    var nodeDocCommentName = _docCommentService.GetNodeName(node);

                    typeNode.AddChild(nodeDocCommentName, node);
                }

                foreach (var methodInfo in methodInfos)
                {
                    if (methodInfo.IsPrivate || methodInfo.IsAssembly)
                    {
                        continue;
                    }

                    var node = new MethodNode(methodInfo);
                    var nodeDocCommentName = _docCommentService.GetNodeName(node);

                    typeNode.AddChild(nodeDocCommentName, node);
                }
            }
        }

        _docCommentService.LoadInto(directory, namespaceNode);

        return namespaceNode;
    }
}