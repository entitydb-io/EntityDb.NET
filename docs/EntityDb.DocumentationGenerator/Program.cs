using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using EntityDb.DocumentationGenerator.Nodes;

var path = AppContext.BaseDirectory;
var directory = new DirectoryInfo(path);

var executingAssembly = Assembly.GetExecutingAssembly();

var types = directory.GetFiles($"EntityDb.*.dll")
    .Where(assemblyFile => !assemblyFile.Name.Contains(executingAssembly.GetName().Name!))
    .Select(assemblyFile => Assembly.LoadFrom(assemblyFile.FullName))
    .SelectMany(assembly => assembly.GetTypes().Where(type => type.IsPublic))
    .ToArray();

// Build Node Tree

var documentationTree = new NamespaceNode();

foreach (var type in types)
{
    var typeNode = new TypeNode(type);

    documentationTree.Add(type.FullName!, typeNode);

    var bindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    foreach (var constructorInfo in typeNode.Type.GetConstructors(bindingFlags))
    {
        if (constructorInfo.IsPrivate || constructorInfo.IsAssembly)
        {
            continue;
        }

        var constructorNode = new ConstructorNode(constructorInfo);

        typeNode.Add(constructorNode.GetXmlDocCommentName(), constructorNode);
    }

    var propertyAccessors = new List<MethodInfo>();

    foreach (var propertyInfo in typeNode.Type.GetProperties(bindingFlags))
    {
        var accessors = propertyInfo.GetAccessors();

        if (accessors.All(methodInfo => methodInfo.IsPrivate || methodInfo.IsAssembly))
        {
            continue;
        }

        var propertyNode = new PropertyNode(propertyInfo);

        typeNode.Add(propertyNode.GetXmlDocCommentName(), propertyNode);

        propertyAccessors.AddRange(accessors);
    }

    foreach (var methodInfo in typeNode.Type.GetMethods(bindingFlags).Except(propertyAccessors))
    {
        if (methodInfo.IsPrivate || methodInfo.IsAssembly)
        {
            continue;
        }

        var methodNode = new MethodNode(methodInfo);

        typeNode.Add(methodNode.GetXmlDocCommentName(), methodNode);
    }
}

var xmlDocCommentTree = new Dictionary<string, Node>();

void BuildXmlDocCommentTree(IDictionary<string, Node> childNodes, string parentName = "")
{
    foreach (var (name, node) in childNodes)
    {
        var xmlDocCommentNamePrefix = node switch
        {
            TypeNode => "T",
            PropertyNode => "P",
            MethodNode or ConstructorNode => "M",
            _ => null
        };

        if (xmlDocCommentNamePrefix != null)
        {
            xmlDocCommentTree.Add($"{xmlDocCommentNamePrefix}:{parentName}{name}", node);
        }

        BuildXmlDocCommentTree(node.ChildNodes, $"{parentName}{name}.");
    }
}

BuildXmlDocCommentTree(documentationTree.ChildNodes, "");

var documentationFiles = directory.GetFiles("EntityDb.*.xml")
    .Select(documentationFile =>
    {
        var document = new XmlDocument();

        using var xmlFile = documentationFile.OpenRead();

        return new XPathDocument(xmlFile).CreateNavigator();
    });

foreach (var documentationFile in documentationFiles)
{
    var members = documentationFile.Select("doc/members/member");

    while (members.MoveNext())
    {
        var member = members.Current!;

        var name = member.GetAttribute("name", "");

        if (xmlDocCommentTree.TryGetValue(name, out var node))
        {
            node.HasXmlDocComment = true;
        }
    }
}

foreach (var (x, n) in xmlDocCommentTree)
{
}