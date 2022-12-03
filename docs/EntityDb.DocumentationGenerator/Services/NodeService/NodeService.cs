using System.Reflection;
using EntityDb.DocumentationGenerator.Nodes;

namespace EntityDb.DocumentationGenerator.Services.NodeService;

internal class NodeService : INodeService
{
    public NamespaceNode GetNamespaceNode(IEnumerable<Type> types)
    {
        var namespaceNode = new NamespaceNode();

        foreach (var type in types)
        {
            var typeNode = new TypeNode(type);

            namespaceNode.AddChild(type.FullName!, typeNode);

            var bindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            var constructorInfos = typeNode.Type
                .GetConstructors(bindingFlags);

            var propertyInfos = typeNode.Type
                .GetProperties(bindingFlags);

            var methodInfos = typeNode.Type
                .GetMethods(bindingFlags)
                .Except(propertyInfos
                    .SelectMany(propertyInfo => propertyInfo.GetAccessors()));

            foreach (var constructorInfo in constructorInfos)
            {
                if (constructorInfo.IsPrivate || constructorInfo.IsAssembly)
                {
                    continue;
                }

                var constructorNode = new ConstructorNode(constructorInfo);

                typeNode.AddChild(constructorNode.GetXmlDocCommentName(), constructorNode);
            }

            foreach (var propertyInfo in propertyInfos)
            {
                var accessors = propertyInfo.GetAccessors();

                if (accessors.All(methodInfo => methodInfo.IsPrivate || methodInfo.IsAssembly))
                {
                    continue;
                }

                var propertyNode = new PropertyNode(propertyInfo);

                typeNode.AddChild(propertyNode.GetXmlDocCommentName(), propertyNode);
            }

            foreach (var methodInfo in methodInfos)
            {
                if (methodInfo.IsPrivate || methodInfo.IsAssembly)
                {
                    continue;
                }

                var methodNode = new MethodNode(methodInfo);

                typeNode.AddChild(methodNode.GetXmlDocCommentName(), methodNode);
            }
        }

        return namespaceNode;
    }
}