using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using EntityDb.DocumentationGenerator.Nodes;

namespace EntityDb.DocumentationGenerator.Services.DocCommentService;

public class DocCommentService : IDocCommentService
{
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

    public void LoadInto(DirectoryInfo directory, NamespaceNode namespaceNode)
    {
        var documentationFiles = directory.GetFiles("EntityDb.*.xml")
            .Select(documentationFile =>
            {
                var document = new XmlDocument();

                using var xmlFile = documentationFile.OpenRead();

                return new XPathDocument(xmlFile).CreateNavigator();
            });

        var flatNodeDictionary = GetFlatNodeDictionary(namespaceNode);

        foreach (var documentationFile in documentationFiles)
        {
            var members = documentationFile.Select("doc/members/member");

            foreach (XPathNavigator member in members)
            {
                var name = member.GetAttribute("name", "");

                if (flatNodeDictionary.TryGetValue(name, out var node) && node is MemberInfoNode memberInfoNode)
                {
                    foreach (XPathNavigator child in member.Select("*"))
                    {
                        memberInfoNode.AddDocumentation(child);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }

    private static Dictionary<string, Node> GetFlatNodeDictionary(NamespaceNode namespaceNode)
    {
        var flatNodeDictionary = new Dictionary<string, Node>();

        BuildFlatNodeDictionary(namespaceNode.GetChildNodes());

        return flatNodeDictionary;

        void BuildFlatNodeDictionary(IEnumerable<KeyValuePair<string, Node>> nodes, string parentName = "")
        {
            foreach (var (name, node) in nodes)
            {
                var xmlDocCommentNamePrefix = node switch
                {
                    TypeNode => "T",
                    PropertyNode => "P",
                    MethodNode or ConstructorNode => "M",
                    FieldNode => "F",
                    _ => null
                };

                if (xmlDocCommentNamePrefix != null)
                {
                    flatNodeDictionary.Add($"{xmlDocCommentNamePrefix}:{parentName}{name}", node);
                }

                if (node is INestableNode nestableNode)
                {
                    BuildFlatNodeDictionary(nestableNode.GetChildNodes(), $"{parentName}{name}.");
                }
            }
        }
    }
}