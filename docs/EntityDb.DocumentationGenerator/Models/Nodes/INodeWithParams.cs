using System.Reflection;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public interface INodeWithParams
{
    public ParameterInfo[] GetParams();
    public MemberParamDoc? GetParamDoc(string paramName);
}
