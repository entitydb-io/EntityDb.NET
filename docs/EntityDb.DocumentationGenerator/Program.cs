using System;
using System.IO;
using System.Linq;
using System.Reflection;

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
    }
}