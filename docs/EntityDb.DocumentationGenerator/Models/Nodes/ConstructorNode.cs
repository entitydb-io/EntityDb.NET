using System.Reflection;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public class ConstructorNode : MemberInfoNode, INodeWithParams
{
    public ConstructorInfo ConstructorInfo { get; }
    public Dictionary<string, MemberTypeParamDoc> TypeParamDocs { get; init; } = new();
    public Dictionary<string, MemberParamDoc> ParamDocs { get; init; } = new();

    public ConstructorNode(ConstructorInfo constructorInfo)
    {
        ConstructorInfo = constructorInfo;
    }

    public ParameterInfo[] GetParams()
    {
        return ConstructorInfo.GetParameters();
    }

    public MemberParamDoc? GetParamDoc(string paramName)
    {
        return ParamDocs.GetValueOrDefault(paramName);
    }

    public override void AddDocumentation(object docCommentMemberItem)
    {
        switch (docCommentMemberItem)
        {
            case MemberIgnoreDoc:
                // Used to make the Warning go away for public constructors on public classes
                //TODO: Consider making these classes internal, provide another way to use them.
                break;

            case MemberTypeParamDoc:
                // These are redundant. The type has these type params.
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