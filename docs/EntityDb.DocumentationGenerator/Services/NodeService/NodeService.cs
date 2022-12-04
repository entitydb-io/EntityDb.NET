﻿using System.Reflection;
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

    private void AddTypeNode(INestableNode parentNode, Type type)
    {
        var typeNode = new TypeNode(type);

        if (parentNode is NamespaceNode)
        {
            parentNode.AddChild(type.FullName!, typeNode);
        }
        else
        {
            parentNode.AddChild(type.Name, typeNode);
        }

        var bindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        var nestedTypes = type
            .GetNestedTypes(bindingFlags)
            .Where(fieldInfo => fieldInfo.GetCustomAttribute<CompilerGeneratedAttribute>() == null);

        foreach (var nestedType in nestedTypes)
        {
            AddTypeNode(typeNode, nestedType);
        }

        var fieldInfos = type
            .GetFields(bindingFlags)
            .Where(fieldInfo => fieldInfo.GetCustomAttribute<CompilerGeneratedAttribute>() == null);

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

        foreach (var fieldInfo in fieldInfos)
        {
            if (fieldInfo.IsPrivate || fieldInfo.IsAssembly)
            {
                continue;
            }

            var node = new FieldNode(fieldInfo);
            var nodeDocCommentName = _docCommentService.GetNodeName(node);

            typeNode.AddChild(nodeDocCommentName, node);
        }

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

    public NamespaceNode GetNamespaceNode(DirectoryInfo directory)
    {
        var namespaceNode = new NamespaceNode();

        var assemblies = _assemblyService.GetAssemblies(directory);

        foreach (var assembly in assemblies)
        {
            var types = assembly.GetTypes()
                .Where(type => type.IsPublic)
                .OrderBy(type => type.Namespace);

            foreach (var type in types)
            {
                AddTypeNode(namespaceNode, type);
            }
        }

        _docCommentService.LoadInto(directory, namespaceNode);

        return namespaceNode;
    }
}