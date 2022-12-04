using System.Reflection;
using System.Xml.Serialization;
using EntityDb.DocumentationGenerator.Models.Nodes;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Services.DocCommentService;

public class XmlDocCommentService : IXmlDocCommentService
{
    private static readonly XmlSerializer _xmlSerializer = new(typeof(DocFile));

    public string GetNodeName(Node node)
    {
        return node switch
        {
            FieldNode fieldNode => GetFieldInfoName(fieldNode.FieldInfo),
            ConstructorNode constructorNode => GetConstructorInfoName(constructorNode.ConstructorInfo),
            MethodNode methodNode => GetMethodInfoName(methodNode.MethodInfo),
            PropertyNode propertyNode => GetPropertyInfoName(propertyNode.PropertyInfo),
            _ => throw new NotImplementedException(),
        };
    }

    private static string GetFieldInfoName(FieldInfo fieldInfo)
    {
        return fieldInfo.Name;
    }

    private static string GetConstructorInfoName(ConstructorInfo constructorInfo)
    {
        var parameterInfos = constructorInfo.GetParameters();

        return GetMemberInfoName(constructorInfo) + GetParameterInfosName(parameterInfos);
    }

    private static string GetMemberInfoName(MemberInfo memberInfo)
    {
        return memberInfo.Name.Replace(".", "#").Replace("&", "@");
    }

    private static string GetMethodInfoName(MethodInfo methodInfo)
    {
        var parameterInfos = methodInfo.GetParameters();

        var genericIndicator = string.Empty;

        if (methodInfo.IsGenericMethod)
        {
            genericIndicator = $"``{methodInfo.GetGenericArguments().Length}";
        }

        var memberInfoName = GetMemberInfoName(methodInfo);

        if (memberInfoName == "op_Implicit")
        {
            return memberInfoName + genericIndicator + GetParameterInfosName(parameterInfos) + "~" + GetTypeName(methodInfo.ReturnType);
        }

        return memberInfoName + genericIndicator + GetParameterInfosName(parameterInfos);
    }

    private static string GetParameterInfosName(ParameterInfo[] parameterInfos)
    {
        if (parameterInfos.Length == 0)
        {
            return "";
        }

        var list = new List<string>();

        foreach (var parameterInfo in parameterInfos)
        {
            list.Add(GetTypeName(parameterInfo.ParameterType));
        }

        return $"({string.Join(",", list)})";
    }

    private static string GetPropertyInfoName(PropertyInfo propertyInfo)
    {
        var indexParameterInfos = propertyInfo.GetIndexParameters();

        if (indexParameterInfos?.Length > 0)
        {
            return GetMemberInfoName(propertyInfo) + GetParameterInfosName(indexParameterInfos);
        }

        return GetMemberInfoName(propertyInfo);
    }

    private static string GetTypeName(Type type)
    {
        if (type.IsArray)
        {
            var elementName = GetTypeName(type.GetElementType()!);

            var arrayIndicator = $"[{string.Join(",", Enumerable.Repeat("", type.GetArrayRank()))}]";

            return $"{elementName}{arrayIndicator}";
        }

        if (type.IsGenericTypeParameter)
        {
            return $"`{type.GenericParameterPosition}";
        }

        if (type.IsGenericMethodParameter)
        {
            return $"``{type.GenericParameterPosition}";
        }

        if (type.IsGenericType)
        {
            var list = new List<string>();

            foreach (var genericArgument in type.GetGenericArguments())
            {
                list.Add(GetTypeName(genericArgument));
            }

            var baseName = type.Name[..type.Name.IndexOf('`')];

            return $"{type.Namespace}.{baseName}{{{string.Join(',', list)}}}";
        }

        if (type.IsNested)
        {
            return $"{GetTypeName(type.DeclaringType!)}.{GetMemberInfoName(type)}";
        }

        return $"{type.Namespace}.{GetMemberInfoName(type)}";
    }

    public DocFile GetDocFile(DirectoryInfo directory, string fileName)
    {
        var fileInfo = directory.GetFiles(fileName)
            .SingleOrDefault();

        if (fileInfo == default)
        {
            throw new Exception($"Cannot locate {fileName} in {directory}");
        }

        return GetDocFile(fileInfo);
    }

    private static DocFile GetDocFile(FileInfo fileInfo)
    {
        return (DocFile)_xmlSerializer.Deserialize(fileInfo.OpenRead())!;
    }
}