using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public interface INodeWithTypeParams
{
    public Type[] GetTypeParams();
    public MemberTypeParamDoc? GetTypeParamDoc(string typeParamName);
}
