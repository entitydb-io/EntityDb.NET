using System.Reflection;
using System.Runtime.Intrinsics.X86;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;
using EntityDb.DocumentationGenerator.Nodes;

namespace EntityDb.DocumentationGenerator.Services.PrintingService;

public class ConsolePrintingService : IPrintingService
{
    public void Print(NamespaceNode namespaceNode)
    {
        Console.WriteLine($"Namespaces");

        PrintNode(0, "", namespaceNode);
    }

    public string ConvertSeeDoc(SeeDoc see)
    {
        return $"<<see:{see.SeeRef}>>";
    }

    public string ConvertParamRefDoc(ParamRefDoc paramRefDoc)
    {
        return $"<<paramRef:{paramRefDoc.Name}>>";
    }

    public string ConvertTypeParamRefDoc(TypeParamRefDoc typeParamRefDoc)
    {
        return $"<<paramRef:{typeParamRefDoc.Name}>>";
    }

    public string ConvertCodeDoc(CodeDoc codeDoc)
    {
        return $"<<code:{codeDoc.GetText(this)}>>";
    }

    private string GetPadding(int depth)
    {
        return $"{string.Join("", Enumerable.Repeat("  ", depth))}";
    }

    private void PrintNodes<TNode>(int depth, string parentPath, string groupName, IDictionary<string, TNode> typeNodes)
        where TNode : INode
    {
        if (typeNodes.Count > 0)
        {
            var padding = GetPadding(depth);

            Console.WriteLine($"{padding}- {groupName}");
        }

        foreach (var (_, typeNode) in typeNodes)
        {
            PrintNode(depth + 1, parentPath, typeNode);
        }
    }

    private void PrintNode(int depth, string parentPath, INode node)
    {
        if (node is NamespaceNode namespaceNode)
        {
            PrintNode(depth + 1, parentPath, namespaceNode.NestedTypesNode);

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
            if (nestedTypesNode.Count > 0 && nestedTypesNode.IsNamespace)
            {
                Console.WriteLine($"{padding1}- {parentPath[1..]}");
            }

            PrintNodes(depth + 1, parentPath, "Classes", nestedTypesNode.ClassNodes);
            PrintNodes(depth + 1, parentPath, "Structs", nestedTypesNode.StructNodes);
            PrintNodes(depth + 1, parentPath, "Interfaces", nestedTypesNode.InterfaceNodes);
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
                Console.WriteLine($"{padding2}- Summary: {memberInfoNode.Summary.GetText(this)}");
            }

            if (memberInfoNode.Remarks != null)
            {
                Console.WriteLine($"{padding2}- Remarks: {memberInfoNode.Remarks.GetText(this)}");
            }
        }

        if (node is MethodNode methodNode && !string.IsNullOrWhiteSpace(methodNode.Returns))
        {
            Console.WriteLine($"{padding2}- Returns: {methodNode.Returns}");
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

    private string GetNodeName(INode node)
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
            .Select(type => GetTypeName(type));

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
}
