using System.Reflection;
using EntityDb.DocumentationGenerator.Nodes;

namespace EntityDb.DocumentationGenerator.Services.PrintingService;

public class ConsolePrintingService : IPrintingService
{
    public void Print(NamespaceNode namespaceNode)
    {
        PrintNamespaceNode(0, "", namespaceNode);
    }

    private static string GetPadding(int depth)
    {
        return $"\n{string.Join("", Enumerable.Repeat("  ", depth))}";
    }

    private static void PrintNamespaceNode(int depth, string fullPath, NamespaceNode namespaceNode)
    {
        int nextNamespaceNodeDepth;

        if (namespaceNode.AssemblyNode != null || namespaceNode.TypeNodeCount > 0)
        {
            Console.WriteLine($"{GetPadding(depth)}- {fullPath[1..]}");

            nextNamespaceNodeDepth = depth + 2;
        }
        else
        {
            nextNamespaceNodeDepth = depth;
        }

        if (namespaceNode.AssemblyNode != null)
        {
            PrintNode(depth + 1, namespaceNode.AssemblyNode);
        }

        PrintNodes(depth + 1, "Classes", namespaceNode.ClassNodes);
        PrintNodes(depth + 1, "Structs", namespaceNode.StructNodes);
        PrintNodes(depth + 1, "Interfaces", namespaceNode.InterfaceNodes);

        if (depth != nextNamespaceNodeDepth)
        {
            Console.WriteLine($"{GetPadding(depth + 1)}- Namespaces:");
        }

        foreach (var (childPath, childNamespaceNode) in namespaceNode.NamespaceNodes)
        {
            PrintNamespaceNode(nextNamespaceNodeDepth, $"{fullPath}.{childPath}", childNamespaceNode);
        }
    }

    private static void PrintNodes<TNode>(int depth, string groupName, IDictionary<string, TNode> typeNodes)
        where TNode : Node
    {
        if (typeNodes.Count > 0)
        {
            var padding = GetPadding(depth);

            Console.WriteLine($"{padding}- {groupName}:");
        }

        foreach (var (_, typeNode) in typeNodes)
        {
            PrintNode(depth + 1, typeNode);
        }
    }

    private static void PrintNode(int depth, Node node)
    {
        var padding1 = GetPadding(depth);
        var padding2 = GetPadding(depth + 1);
        var padding3 = GetPadding(depth + 2);

        if (node is AssemblyNode assemblyNode)
        {
            Console.WriteLine($"{padding1}- Description: {assemblyNode.GetDescription()}");
        }
        else
        {
            var nodeName = GetNodeName(node);

            Console.WriteLine($"{padding1}- {nodeName}");
        }

        if (node is MemberInfoNode memberInfoNode)
        {
            if (memberInfoNode.TypeParams.Count > 0)
            {
                Console.WriteLine($"{padding2}- TypeParams");

                foreach (var (typeParamName, typeParamDesc) in memberInfoNode.TypeParams)
                {
                    Console.WriteLine($"{padding3}- {typeParamName}: {typeParamDesc}");
                }
            }

            if (memberInfoNode.Params.Count > 0)
            {
                Console.WriteLine($"{padding2}- Params");

                foreach (var (paramName, paramDesc) in memberInfoNode.Params)
                {
                    Console.WriteLine($"{padding3}- {paramName}: {paramDesc}");
                }
            }

            if (memberInfoNode.Summary != null)
            {
                Console.WriteLine($"{padding2}- Summary: {memberInfoNode.Summary}");
            }

            if (memberInfoNode.Remarks != null)
            {
                Console.WriteLine($"{padding2}- Remarks: {memberInfoNode.Remarks}");
            }
        }

        if (node is MethodNode methodNode && !string.IsNullOrWhiteSpace(methodNode.Returns))
        {
            Console.WriteLine($"{padding2}- Returns: {methodNode.Returns}");
        }

        if (node is TypeNode typeNode)
        {
            PrintNodes(depth + 1, "Fields", typeNode.FieldNodes);
            PrintNodes(depth + 1, "Constructors", typeNode.ConstructorNodes);
            PrintNodes(depth + 1, "Properties", typeNode.PropertyNodes);
            PrintNodes(depth + 1, "Methods", typeNode.MethodNodes);
        }
    }

    private static string GetNodeName(Node node)
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

    private static string GetTypesName(IEnumerable<Type> types)
    {
        var typeNames = types
            .Select(type => GetTypeName(type));

        return $"<{string.Join(",", typeNames)}>";
    }

    private static string GetParameterInfosName(IEnumerable<ParameterInfo> parameterInfos)
    {
        var parameterTypeNames = parameterInfos
            .Select(parameterInfo => GetTypeName(parameterInfo.ParameterType));

        return $"({string.Join(", ", parameterTypeNames)})";
    }

    private static string GetConstructorInfoName(ConstructorInfo constructorInfo)
    {
        return $"{GetTypeName(constructorInfo.DeclaringType!)}{GetParameterInfosName(constructorInfo.GetParameters())}";
    }

    private static string GetMethodInfoName(MethodInfo methodInfo)
    {
        return $"{GetTypeName(methodInfo.ReturnType)} {methodInfo.Name}{GetParameterInfosName(methodInfo.GetParameters())}";
    }

    private static string GetPropertyInfoName(PropertyInfo propertyInfo)
    {
        return $"{GetTypeName(propertyInfo.PropertyType)} {propertyInfo.Name}";
    }

    private static string GetFieldInfoName(FieldInfo fieldInfo)
    {
        return $"{GetTypeName(fieldInfo.FieldType)} {fieldInfo.Name}";
    }

    private static string GetTypeName(Type type)
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
}
