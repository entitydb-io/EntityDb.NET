using EntityDb.DocumentationGenerator.Models.XmlDocComment;
using System.Reflection;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public class TypeNode : MemberInfoNode, INestableNode, INodeWithTypeParams
{
    public Type Type { get; }
    public Dictionary<string, MemberTypeParamDoc> TypeParamDocs { get; init; } = new();
    public NestedTypesNode NestedTypesNode { get; init; } = new(false);
    public Dictionary<string, FieldNode> FieldNodes { get; init; } = new();
    public Dictionary<string, ConstructorNode> ConstructorNodes { get; init; } = new();
    public Dictionary<string, PropertyNode> PropertyNodes { get; init; } = new();
    public Dictionary<string, MethodNode> MethodNodes { get; init; } = new();

    public TypeNode(Type type)
    {
        Type = type;
    }

    public Type[] GetTypeParams()
    {
        return Type.GetGenericArguments();
    }

    public MemberTypeParamDoc? GetTypeParamDoc(string typeParamName)
    {
        return TypeParamDocs.GetValueOrDefault(typeParamName);
    }

    public void AddChild(string path, INode node)
    {
        switch (node)
        {
            case TypeNode typeNode:
                NestedTypesNode.AddChild(path, typeNode);
                break;

            case FieldNode fieldNode:
                FieldNodes.Add(path, fieldNode);
                break;

            case ConstructorNode constructorNode:
                ConstructorNodes.Add(path, constructorNode);
                break;

            case PropertyNode propertyNode:
                PropertyNodes.Add(path, propertyNode);
                break;

            case MethodNode methodNode:
                MethodNodes.Add(path, methodNode);
                break;

            default:
                throw new NotImplementedException();
        }
    }

    public override void AddDocumentation(object docCommentMemberItem)
    {
        switch (docCommentMemberItem)
        {
            case MemberTypeParamDoc typeParamDoc:
                TypeParamDocs.Add(typeParamDoc.Name, typeParamDoc);
                break;

            case MemberParamDoc:
                //TODO: Figure out how to display these. It appears to be the primary constructor params.
                break;

            default:
                base.AddDocumentation(docCommentMemberItem);
                break;
        }
    }

    public IEnumerable<KeyValuePair<string, INode>> GetChildNodes()
    {
        foreach (var childNode in NestedTypesNode.GetChildNodes())
        {
            yield return childNode;
        }

        foreach (var (path, node) in FieldNodes)
        {
            yield return new(path, node);
        }

        foreach (var (path, node) in ConstructorNodes)
        {
            yield return new(path, node);
        }

        foreach (var (path, node) in PropertyNodes)
        {
            yield return new(path, node);
        }

        foreach (var (path, node) in MethodNodes)
        {
            yield return new(path, node);
        }
    }
}