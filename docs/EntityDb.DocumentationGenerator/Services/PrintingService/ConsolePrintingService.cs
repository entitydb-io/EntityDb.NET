using System.Diagnostics;
using System.Reflection;
using System.Text;
using EntityDb.DocumentationGenerator.Nodes;

namespace EntityDb.DocumentationGenerator.Services.PrintingService;

public class ConsolePrintingService : IPrintingService
{
    public void Print(NamespaceNode namespaceNode)
    {
        Console.WriteLine("Namespaces:");

        PrintNamespaceNode("", namespaceNode);
    }

    private static string GetPadding(int depth)
    {
        return $"\n{string.Join("", Enumerable.Repeat("  ", depth))}";
    }

    private static void PrintNamespaceNode(string fullPath, NamespaceNode namespaceNode)
    {
        foreach (var (childPath, childNamespaceNode) in namespaceNode.NamespaceNodes)
        {
            PrintNamespaceNode($"{fullPath}.{childPath}", childNamespaceNode);
        }

        if (namespaceNode.TypeNodeCount > 0)
        {
            Console.WriteLine("");
            Console.WriteLine($"- {fullPath[1..]}");
        }

        PrintNodes(1, "Classes", namespaceNode.ClassNodes);
        PrintNodes(1, "Structs", namespaceNode.StructNodes);
        PrintNodes(1, "Interfaces", namespaceNode.InterfaceNodes);
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

        var nodeName = GetNodeName(node);

        Console.WriteLine($"{padding1}- {nodeName}");

        if (node.Summary != null)
        {
            Console.WriteLine($"{padding2}- Summary: {node.Summary}");
        }

        if (node.Remarks != null)
        {
            Console.WriteLine($"{padding2}- Remarks: {node.Remarks}");
        }

        if (node is TypeNode typeNode)
        {
            PrintNodes(depth + 1, "Constructors", typeNode.ConstructorNodes);
            PrintNodes(depth + 1, "Properties", typeNode.PropertyNodes);
            PrintNodes(depth + 1, "Methods", typeNode.MethodNodes);
        }

        if (node is MethodNode methodNode && !string.IsNullOrWhiteSpace(methodNode.Returns))
        {
            Console.WriteLine($"{padding2}- Returns: {methodNode.Returns}");
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
        }
    }

    private static string GetNodeName(Node node)
    {
        return node switch
        {
            ConstructorNode constructorNode => GetMethodBaseName(constructorNode.ConstructorInfo),
            MethodNode methodNode => GetMethodBaseName(methodNode.MethodInfo),
            TypeNode typeNode => GetTypeName(typeNode.Type),
            PropertyNode propertyNode => propertyNode.PropertyInfo.Name,
            _ => throw new NotImplementedException(),
        };
    }

    private static string GetMethodBaseName(MethodBase methodBase)
    {
        var declaringTypeName = GetTypeName(methodBase.DeclaringType!);

        var parameterTypeNames = methodBase.GetParameters()
            .Select(parameterInfo => GetTypeName(parameterInfo.ParameterType));

        var methodName = $"{declaringTypeName}({string.Join(", ", parameterTypeNames)})";

        return methodName;
    }

    private static string GetTypeName(Type type)
    {
        if (type.IsByRef)
        {
            return GetTypeName(type.GetElementType()!);
        }

        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var nameBuilder = new StringBuilder();

        var typeNamePrefix = type.Name.Split('`', 2)[0];

        var genericArgumentNames = type.GetGenericArguments()
            .Select(genericArgument => genericArgument.Name);

        var typeName = $"{typeNamePrefix}<{string.Join(",", genericArgumentNames)}>";

        return typeName;
    }
}
