using System.Reflection;
using EntityDb.DocumentationGenerator.Models.Nodes;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Services.PrintingService.ConsolePrinting;

public class ConsolePrintingService : IPrintingService
{
    private readonly Nodes _nodes;

    public ConsolePrintingService(Nodes nodes)
    {
        _nodes = nodes;
    }

    public void Print()
    {
        PrintNode(0, "", _nodes.Root);
    }

    public string ConvertSeeDoc(SeeDoc seeDoc)
    {
        if (_nodes.XmlDocCommentMemberDictionary.TryGetValue(seeDoc.SeeRef, out var seeNode))
        {
            return GetNodeName(seeNode);
        }

        return $"[see external:{seeDoc.SeeRef}]";
    }

    public string ConvertParamRefDoc(ParamRefDoc paramRefDoc)
    {
        return paramRefDoc.GetText(this);
    }

    public string ConvertTypeParamRefDoc(TypeParamRefDoc typeParamRefDoc)
    {
        return typeParamRefDoc.GetText(this);
    }

    public string ConvertCodeDoc(CodeDoc codeDoc)
    {
        return $"`{codeDoc.GetText(this)}`";
    }

    public string ConvertSummaryDoc(SummaryDoc? summaryDoc)
    {
        return summaryDoc?.GetText(this) ?? "Missing Summary Doc!";
    }

    public string ConvertTypeParamDoc(TypeParamDoc? typeParamDoc)
    {
        return typeParamDoc?.GetText(this) ?? "Missing TypeParam Doc!";
    }

    public string ConvertParamDoc(ParamDoc? paramDoc)
    {
        return paramDoc?.GetText(this) ?? "Missing Param Doc!";
    }

    public string? ConvertInheritDoc(InheritDoc? inheritDoc)
    {
        if (inheritDoc == null)
        {
            return null;
        }

        if (inheritDoc.SeeRef == null)
        {
            return "[inherit doc of ancestor]";
        }

        if (_nodes.XmlDocCommentMemberDictionary.TryGetValue(inheritDoc.SeeRef, out var seeNode))
        {
            return GetNodeName(seeNode);
        }

        return $"[inherit external:{inheritDoc.SeeRef}]";
    }

    private void PrintNodes<TNode>(int depth, string parentPath, string groupName, IDictionary<string, TNode> typeNodes)
        where TNode : Node
    {
        var nonIgnoredTypeNodes = typeNodes.Where(pair => !pair.Value.Ignore).ToArray();

        if (nonIgnoredTypeNodes.Length > 0)
        {
            var padding = GetPadding(depth);

            Console.WriteLine($"{padding}- {groupName}");
        }

        foreach (var (_, typeNode) in nonIgnoredTypeNodes)
        {
            PrintNode(depth + 1, parentPath, typeNode);
        }
    }

    private void PrintNode(int depth, string parentPath, Node node)
    {
        if (node.Ignore)
        {
            return;
        }

        if (node is NamespaceNode namespaceNode)
        {
            if (namespaceNode.NestedTypesNode.Count > 0)
            {
                Console.WriteLine($"\n{parentPath[1..]}");
                Console.WriteLine(ConvertSummaryDoc(namespaceNode.SummaryDoc));
            }

            PrintNode(depth, parentPath, namespaceNode.NestedTypesNode);

            foreach (var (childPath, childNamespaceNode) in namespaceNode.NamespaceNodes)
            {
                PrintNode(0, $"{parentPath}.{childPath}", childNamespaceNode);
            }

            return;
        }

        var padding1 = GetPadding(depth);
        var padding2 = GetPadding(depth + 1);
        var padding3 = GetPadding(depth + 2);

        if (node is NestedTypesNode nestedTypesNode)
        {
            PrintNodes(depth + 1, parentPath, "Classes", nestedTypesNode.ClassNodes);
            PrintNodes(depth + 1, parentPath, "Structs", nestedTypesNode.StructNodes);
            PrintNodes(depth + 1, parentPath, "Interfaces", nestedTypesNode.InterfaceNodes);
        }
        else
        {
            var nodeName = GetNodeName(node);

            Console.WriteLine($"{padding1}- {nodeName}");
        }

        if (node.SummaryDoc != null)
        {
            Console.WriteLine($"{padding2}- Summary: {node.SummaryDoc.GetText(this)}");
        }

        if (node.RemarksDoc != null)
        {
            Console.WriteLine($"{padding2}- Remarks: {node.RemarksDoc.GetText(this)}");
        }

        if (node is MethodNode methodNode && methodNode.Returns != null)
        {
            Console.WriteLine($"{padding2}- Returns: {methodNode.Returns.GetText(this)}");
        }

        if (node is INodeWithTypeParams nodeWithTypeParams)
        {
            var typeParams = nodeWithTypeParams.GetTypeParams();

            if (typeParams.Length > 0)
            {
                Console.WriteLine($"{padding2}- TypeParams");

                foreach (var typeParam in typeParams)
                {
                    var typeParamDoc = nodeWithTypeParams.GetTypeParamDoc(typeParam.Name);

                    var typeParamDescription = ConvertInheritDoc(node.InheritDoc) ?? ConvertTypeParamDoc(typeParamDoc);

                    Console.WriteLine($"{padding3}- {typeParam.Name}: {typeParamDescription}");
                }
            }
        }

        if (node is INodeWithParams nodeWithParams)
        {
            var @params = nodeWithParams.GetParams();

            if (@params.Length > 0)
            {
                Console.WriteLine($"{padding2}- Params");

                foreach (var param in @params)
                {
                    var paramDoc = nodeWithParams.GetParamDoc(param.Name!);

                    var paramDescription = ConvertInheritDoc(node.InheritDoc) ?? ConvertParamDoc(paramDoc);

                    Console.WriteLine($"{padding3}- {param.Name}: {paramDescription}");
                }
            }
        }

        if (node is TypeNode typeNode)
        {
            PrintNode(depth, parentPath, typeNode.NestedTypesNode);
            PrintNodes(depth + 1, parentPath, "Fields", typeNode.FieldNodes);
            PrintNodes(depth + 1, parentPath, "Constructors", typeNode.ConstructorNodes);
            PrintNodes(depth + 1, parentPath, "Properties", typeNode.PropertyNodes);
            PrintNodes(depth + 1, parentPath, "Methods", typeNode.MethodNodes);
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

    private string GetTypesName(IEnumerable<Type> types)
    {
        var typeNames = types
            .Select(GetTypeName);

        return $"<{string.Join(",", typeNames)}>";
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
            return type.FullName ?? type.Name;
        }

        var typeNamePrefix = type.Name.Split('`', 2)[0];

        return $"{typeNamePrefix}{GetTypesName(type.GetGenericArguments())}";
    }

    private static string GetPadding(int depth)
    {
        return $"{string.Join("", Enumerable.Repeat("  ", depth))}";
    }
}
