using System.IO;
using System.IO.Pipelines;
using System.Reflection;
using EntityDb.DocumentationGenerator.Extensions;
using EntityDb.DocumentationGenerator.Models.Nodes;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Services.PrintingService.MarkdownPrinting;

internal class NamespaceGitHubPrintingService : IPrintingService
{
    private readonly Nodes _nodes;
    private readonly string _fileNamePrefix;
    private readonly DirectoryInfo _directory;
    private readonly string _primaryCategory;

    public NamespaceGitHubPrintingService(Nodes nodes, string fileNamePrefix, DirectoryInfo directory, string primaryCategory)
    {
        _nodes = nodes;
        _fileNamePrefix = fileNamePrefix;
        _directory = directory;
        this._primaryCategory = primaryCategory;
    }

    public string ConvertCodeDoc(CodeDoc codeDoc)
    {
        return $"`{codeDoc.GetText(this)}`";
    }

    public string ConvertInheritDoc(InheritDoc inheritDoc)
    {
        throw new NotImplementedException();
    }

    public string ConvertParamRefDoc(ParamRefDoc paramRefDoc)
    {
        return $"`{paramRefDoc.GetText(this)}`";
    }

    public string ConvertSeeDoc(SeeDoc seeDoc)
    {
        if (_nodes.XmlDocCommentMemberDictionary.TryGetValue(seeDoc.SeeRef, out var seeNode))
        {
            return GetNodeLink(seeNode);
        }

        return $"[see external:{seeDoc.SeeRef}]";
    }

    public string ConvertTypeParamRefDoc(TypeParamRefDoc typeParamRefDoc)
    {
        return $"`{typeParamRefDoc.GetText(this)}`";
    }

    private string ConvertSummaryDoc(SummaryDoc? summaryDoc)
    {
        return summaryDoc?.GetText(this).TrimMultiline() ?? "Missing Summary Doc!";
    }

    public void Print()
    {
        var fileName = Path.Combine(_directory.FullName!, $"2022-12-04-{_fileNamePrefix}.md");

        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        using var file = File.OpenWrite(fileName);
        using var fileWriter = new StreamWriter(file);

        PrintHeader(fileWriter, "Assemblies");

        PrintNode(fileWriter, "", _nodes.Root);
    }

    private void PrintHeader(StreamWriter fileWriter, string title)
    {
        fileWriter.WriteLine("---");
        fileWriter.WriteLine($"title: {title}");
        fileWriter.WriteLine($"date: {DateTime.UtcNow:yyyy-MM-dd hh:mm:ss zzz}");
        fileWriter.WriteLine($"categories: [{_primaryCategory}]");
        fileWriter.WriteLine($"tags: []");
        fileWriter.WriteLine("---");
        fileWriter.WriteLine();
    }

    private void PrintAssembliesList(StreamWriter fileWriter, string parentPath)
    {
        var assemblyLabel = parentPath[1..];
        var assemblyKey = assemblyLabel.ToLowerInvariant().Replace(".", "-");

        fileWriter.WriteLine($"- [{assemblyLabel}](/posts/{_fileNamePrefix}-{assemblyKey})");
    }

    private void PrintNamespacesTable(StreamWriter fileWriter, string parentPath, NamespaceNode namespaceNode)
    {
        if (namespaceNode.NestedTypesNode.Count > 0)
        {
            var namespaceLabel = parentPath[1..];
            var namespaceKey = namespaceLabel.ToLowerInvariant().Replace(".", "-");

            fileWriter.Write("<tr>");

            fileWriter.Write($"<td><a href='/posts/{_fileNamePrefix}-{namespaceKey}'>{namespaceLabel}</a></td>");

            fileWriter.Write($"<td>{ConvertSummaryDoc(namespaceNode.SummaryDoc)}</td>");

            fileWriter.Write("</tr>");
        }

        foreach (var (childPath, childNamespaceNode) in namespaceNode.NamespaceNodes)
        {
            PrintNamespacesTable(fileWriter, $"{parentPath}.{childPath}", childNamespaceNode);
        }
    }

    private void PrintTypeNodesTable(StreamWriter streamWriter, string groupName, IDictionary<string, TypeNode> typeNodes)
    {
        var nonIgnoredTypeNodes = typeNodes.Where(pair => !pair.Value.Ignore).ToArray();

        if (nonIgnoredTypeNodes.Length == 0)
        {
            return;
        }

        streamWriter.WriteLine($"## {groupName}");

        streamWriter.Write("<table>");

        foreach (var (_, typeNode) in nonIgnoredTypeNodes)
        {
            streamWriter.Write("<tr>");

            streamWriter.Write($"<td>{GetNodeLink(typeNode)}</td>");

            streamWriter.Write($"<td>{ConvertSummaryDoc(typeNode.SummaryDoc)}</td>");

            streamWriter.Write("</tr>");
        }

        streamWriter.WriteLine("</table>");
    }

    private void PrintNode(StreamWriter assembliesFileWriter, string parentPath, Node node)
    {
        if (node.Ignore)
        {
            return;
        }

        if (node is NamespaceNode namespaceNode)
        {
            if (namespaceNode.NestedTypesNode.Count > 0)
            {
                var namespaceLabel = parentPath[1..];
                var namespaceKey = namespaceLabel.ToLowerInvariant().Replace(".", "-");

                var fileName = Path.Combine(_directory.FullName!, $"2022-12-04-{_fileNamePrefix}-{namespaceKey}.md");

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                using var file = File.OpenWrite(fileName);
                using var fileWriter = new StreamWriter(file);

                PrintHeader(fileWriter, $"{namespaceLabel} Namespace");

                fileWriter.WriteLine(ConvertSummaryDoc(namespaceNode.SummaryDoc));

                PrintTypeNodesTable(fileWriter, "Classes", namespaceNode.NestedTypesNode.ClassNodes);
                PrintTypeNodesTable(fileWriter, "Structs", namespaceNode.NestedTypesNode.StructNodes);
                PrintTypeNodesTable(fileWriter, "Interfaces", namespaceNode.NestedTypesNode.InterfaceNodes);
            }
            
            if (namespaceNode.AssemblyNode != null)
            {
                PrintAssembliesList(assembliesFileWriter, parentPath);

                var assemblyLabel = parentPath[1..];
                var assemblyKey = assemblyLabel.ToLowerInvariant().Replace(".", "-");

                var fileName = Path.Combine(_directory.FullName!, $"2022-12-04-{_fileNamePrefix}-{assemblyKey}.md");

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                using var file = File.OpenWrite(fileName);
                using var fileWriter = new StreamWriter(file);

                PrintHeader(fileWriter, $"{assemblyLabel} Assembly");

                fileWriter.WriteLine("## Installation");

                fileWriter.WriteLine("```sh");
                fileWriter.WriteLine($"dotnet add package {assemblyLabel}");
                fileWriter.WriteLine("```");

                fileWriter.WriteLine($"## Namespaces");

                fileWriter.Write("<table>");

                PrintNamespacesTable(fileWriter, parentPath, namespaceNode);

                fileWriter.WriteLine("</table>");
            }

            foreach (var (childPath, childNamespaceNode) in namespaceNode.NamespaceNodes)
            {
                PrintNode(assembliesFileWriter, $"{parentPath}.{childPath}", childNamespaceNode);
            }
        }
    }

    private string GetNodeName(Node node)
    {
        return node switch
        {
            ConstructorNode constructorNode => GetConstructorInfoName(constructorNode.ConstructorInfo),
            MethodNode methodNode => GetMethodInfoName(methodNode.MethodInfo),
            TypeNode typeNode => GetTypeName(typeNode.Type),
            PropertyNode propertyNode => GetPropertyInfoName(propertyNode.PropertyInfo),
            FieldNode fieldNode => GetFieldInfoName(fieldNode.FieldInfo),
            _ => throw new NotImplementedException(),
        };
    }

    private string GetNodeKey(Node node)
    {
        return (node switch
        {
            ConstructorNode constructorNode => $"{constructorNode.ConstructorInfo.DeclaringType!.FullName!}-.ctor#...",
            MethodNode methodNode => $"{methodNode.MethodInfo.DeclaringType!.FullName!}.{methodNode.MethodInfo.Name}",
            TypeNode typeNode => typeNode.Type.FullName!,
            PropertyNode propertyNode => $"{propertyNode.PropertyInfo.DeclaringType!.FullName!}.{propertyNode.PropertyInfo.Name}",
            FieldNode fieldNode => $"{fieldNode.FieldInfo.DeclaringType!.FullName!}.{fieldNode.FieldInfo.Name}",
            _ => throw new NotImplementedException(),
        }).ToLowerInvariant().Replace(".", "-");
    }

    private string GetNodeLink(Node node)
    {
        var nodeName = GetNodeName(node);
        var nodeKey = GetNodeKey(node);

        return $"<!--/posts/{_fileNamePrefix}-{nodeKey}--><a href='#'>{nodeName}</a>";
    }

    private string GetTypesName(IEnumerable<Type> types)
    {
        var typeNames = types
            .Select(GetTypeName);

        return $"&lt;{string.Join(",", typeNames)}&gt;";
    }

    private string GetParameterInfosName(IEnumerable<ParameterInfo> parameterInfos)
    {
        var parameterTypeNames = parameterInfos
            .Select(parameterInfo => GetTypeName(parameterInfo.ParameterType));

        return $"({string.Join(", ", parameterTypeNames)})";
    }

    private string GetConstructorInfoName(ConstructorInfo constructorInfo)
    {
        return $"{GetTypeName(constructorInfo.DeclaringType!)}{GetParameterInfosName(constructorInfo.GetParameters())}";
    }

    private string GetMethodInfoName(MethodInfo methodInfo)
    {
        return $"{GetTypeName(methodInfo.ReturnType)} {methodInfo.Name}{GetParameterInfosName(methodInfo.GetParameters())}";
    }

    private string GetPropertyInfoName(PropertyInfo propertyInfo)
    {
        return $"{GetTypeName(propertyInfo.PropertyType)} {propertyInfo.Name}";
    }

    private string GetFieldInfoName(FieldInfo fieldInfo)
    {
        return $"{GetTypeName(fieldInfo.FieldType)} {fieldInfo.Name}";
    }

    private string GetTypeName(Type type)
    {
        if (type.IsByRef)
        {
            return GetTypeName(type.GetElementType()!);
        }

        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var typeNamePrefix = type.Name.Split('`', 2)[0];

        return $"{typeNamePrefix}{GetTypesName(type.GetGenericArguments())}";
    }
}
