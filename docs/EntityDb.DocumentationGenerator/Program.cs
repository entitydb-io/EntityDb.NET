using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;
using EntityDb.DocumentationGenerator.Nodes;

var path = AppContext.BaseDirectory;
var directory = new DirectoryInfo(path);

var executingAssembly = Assembly.GetExecutingAssembly();

var assemblies = directory.GetFiles("EntityDb.*.dll")
    .Where(assemblyFile => !assemblyFile.Name.Contains(executingAssembly.GetName().Name!))
    .Select(assemblyFile => Assembly.LoadFrom(assemblyFile.FullName))
    .ToArray();

// Build Node Tree

var rootNode = new NamespaceNode();

foreach (var assembly in assemblies)
{
    var assemblyNode = new AssemblyNode(assembly);

    var assemblyName = assembly.GetName().Name!;

    rootNode.Add(assemblyName, assemblyNode);

    foreach (var type in assembly.GetTypes().Where(type => type.Namespace!.StartsWith(assemblyName)))
    {
        var typeNode = new TypeNode(type);

        rootNode.Add(type.FullName!, typeNode);

        var propertyAccessors = type.GetProperties()
            .SelectMany(propertyInfo => propertyInfo.GetAccessors());

        var eventAccessors = type.GetEvents()
            .SelectMany(eventInfo => new[]
            {
                eventInfo.GetAddMethod(),
                eventInfo.GetRemoveMethod()
            });

        var members = type.GetMembers()
            .Except(propertyAccessors)
            .Except(eventAccessors);

        foreach (var member in members)
        {
            MemberInfoNode memberInfoNode = member switch
            {
                ConstructorInfo constructorInfo => new ConstructorNode(constructorInfo),
                EventInfo eventInfo => new EventNode(eventInfo),
                FieldInfo fieldInfo => new FieldNode(fieldInfo),
                MethodInfo methodInfo => new MethodNode(methodInfo),
                PropertyInfo propertyInfo => new PropertyNode(propertyInfo),
                TypeInfo typeInfo => new TypeInfoNode(typeInfo),
                _ => throw new InvalidOperationException()
            };

            try
            {
                typeNode.Add(memberInfoNode.GetName(), memberInfoNode);
            }
            catch
            {
            }
        }
    }
}

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
    } 
}