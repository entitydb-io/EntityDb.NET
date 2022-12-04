using System.Reflection;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public class MethodNode : MemberInfoNode, INodeWithTypeParams, INodeWithParams
{
    public MethodInfo MethodInfo { get; }
    public Dictionary<string, MemberTypeParamDoc> TypeParamDocs { get; init; } = new();
    public Dictionary<string, MemberParamDoc> ParamDocs { get; init; } = new();

    public MemberReturnsDoc? Returns { get; set; }

    public MethodNode(MethodInfo methodInfo)
    {
        MethodInfo = methodInfo;
    }

    public Type[] GetTypeParams()
    {
        return MethodInfo.GetGenericArguments();
    }

    public MemberTypeParamDoc? GetTypeParamDoc(string typeParamName)
    {
        return TypeParamDocs.GetValueOrDefault(typeParamName);
    }

    public ParameterInfo[] GetParams()
    {
        return MethodInfo.GetParameters();
    }

    public MemberParamDoc? GetParamDoc(string paramName)
    {
        return ParamDocs.GetValueOrDefault(paramName);
    }

    public override void AddDocumentation(object docCommentMemberItem)
    {
        switch (docCommentMemberItem)
        {
            case MemberReturnsDoc returnsDoc:
                Returns = returnsDoc;
                break;

            case MemberTypeParamDoc typeParamDoc:
                TypeParamDocs.Add(typeParamDoc.Name, typeParamDoc);
                break;

            case MemberParamDoc paramDoc:
                ParamDocs.Add(paramDoc.Name, paramDoc);
                break;

            default:
                base.AddDocumentation(docCommentMemberItem);
                break;
        }
    }
}