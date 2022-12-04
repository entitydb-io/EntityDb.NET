using System.Reflection;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public class MethodNode : Node, INodeWithTypeParams, INodeWithParams
{
    public MethodInfo MethodInfo { get; }
    public Dictionary<string, TypeParamDoc> TypeParamDocs { get; init; } = new();
    public Dictionary<string, ParamDoc> ParamDocs { get; init; } = new();

    public ReturnsDoc? Returns { get; set; }

    public MethodNode(MethodInfo methodInfo)
    {
        MethodInfo = methodInfo;
    }

    public Type[] GetTypeParams()
    {
        return MethodInfo.GetGenericArguments();
    }

    public TypeParamDoc? GetTypeParamDoc(string typeParamName)
    {
        return TypeParamDocs.GetValueOrDefault(typeParamName);
    }

    public ParameterInfo[] GetParams()
    {
        return MethodInfo.GetParameters();
    }

    public ParamDoc? GetParamDoc(string paramName)
    {
        return ParamDocs.GetValueOrDefault(paramName);
    }

    public override void AddDocumentation(object docCommentMemberItem)
    {
        switch (docCommentMemberItem)
        {
            case ReturnsDoc returnsDoc:
                Returns = returnsDoc;
                break;

            case TypeParamDoc typeParamDoc:
                TypeParamDocs.Add(typeParamDoc.Name, typeParamDoc);
                break;

            case ParamDoc paramDoc:
                ParamDocs.Add(paramDoc.Name, paramDoc);
                break;

            default:
                base.AddDocumentation(docCommentMemberItem);
                break;
        }
    }
}