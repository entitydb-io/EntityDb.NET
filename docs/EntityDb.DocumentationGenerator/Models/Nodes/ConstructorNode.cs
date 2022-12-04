using System.Reflection;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public class ConstructorNode : Node, INodeWithParams
{
    public ConstructorInfo ConstructorInfo { get; }
    public Dictionary<string, TypeParamDoc> TypeParamDocs { get; init; } = new();
    public Dictionary<string, ParamDoc> ParamDocs { get; init; } = new();

    public ConstructorNode(ConstructorInfo constructorInfo)
    {
        ConstructorInfo = constructorInfo;
    }

    public ParameterInfo[] GetParams()
    {
        return ConstructorInfo.GetParameters();
    }

    public ParamDoc? GetParamDoc(string paramName)
    {
        return ParamDocs.GetValueOrDefault(paramName);
    }

    public override void AddDocumentation(object docCommentMemberItem)
    {
        switch (docCommentMemberItem)
        {
            case TypeParamDoc:
                // These are redundant. The type has these type params.
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