using System.Diagnostics;
using System.Reflection;
using System.Text;
using EntityDb.DocumentationGenerator.Nodes;

namespace EntityDb.DocumentationGenerator.Services.PrintingService;

public class ConsolePrintingService : IPrintingService
{
    public void Print(NamespaceNode namespaceNode)
    {
        Print(0, "", namespaceNode);
    }

    private void Print(int depth, string path, Node node)
    {
        var nodeName = GetNodeName(path, node);
        var prefix = string.Join("", Enumerable.Repeat(" ", depth));

        Console.WriteLine($"{prefix}{nodeName}");

        if (node.Summary != null)
        {
            Console.WriteLine($"{prefix}- Summary: {node.Summary}");
        }

        if (node.Remarks != null)
        {
            Console.WriteLine($"{prefix}- Remarks: {node.Remarks}");
        }

        if (node is MethodNode methodNode)
        {
            if (methodNode.Returns != null)
            {
                Console.WriteLine($"{prefix}- Returns: {methodNode.Returns}");
            }
        }

        if (node is MemberInfoNode memberInfoNode)
        {
            if (memberInfoNode.TypeParams.Count > 0)
            {
                Console.WriteLine($"{prefix}- TypeParams");

                foreach (var (typeParamName, typeParamDesc) in memberInfoNode.TypeParams)
                {
                    Console.WriteLine($"{prefix}  - {typeParamName}: {typeParamDesc}");
                }
            }

            if (memberInfoNode.Params.Count > 0)
            {
                Console.WriteLine($"{prefix}- Params");

                foreach (var (paramName, paramDesc) in memberInfoNode.Params)
                {
                    Console.WriteLine($"{prefix}  - {paramName}: {paramDesc}");
                }
            }
        }

        Console.WriteLine("");

        foreach (var (childPath, childNode) in node.ChildNodes)
        {
            Print(depth + 1, childPath, childNode);
        }
    }

    private static string GetNodeName(string path, Node node)
    {
        return node switch
        {
            ConstructorNode constructorNode => GetMethodBaseName(constructorNode.ConstructorInfo),
            MethodNode methodNode => GetMethodBaseName(methodNode.MethodInfo),
            TypeNode typeNode => GetTypeName(typeNode.Type),
            PropertyNode propertyNode => propertyNode.PropertyInfo.Name,
            NamespaceNode => path,
            _ => throw new UnreachableException(),
        };
    }

    private static string GetMethodBaseName(MethodBase methodBase)
    {
        var declaringTypeName = GetTypeName(methodBase.DeclaringType!);

        var parameterTypeNames = methodBase.GetParameters()
            .Select(parameterInfo => GetTypeName(parameterInfo.ParameterType));

        return $"{declaringTypeName}({string.Join(",", parameterTypeNames)})";
    }

    private static string GetTypeName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var nameBuilder = new StringBuilder();

        var typeNamePrefix = type.Name.Split('`', 2)[0];

        var genericArgumentNames = type.GetGenericArguments()
            .Select(genericArgument => genericArgument.Name);

        return $"{typeNamePrefix}<{string.Join(",", genericArgumentNames)}>";
    }
}
