using System.Xml.XPath;

namespace EntityDb.DocumentationGenerator.Nodes;

public abstract class MemberInfoNode : Node
{
    public Dictionary<string, string> TypeParams { get; init; } = new();
    public Dictionary<string, string> Params { get; init; } = new();
    public string? Summary { get; set; }
    public string? Remarks { get; set; }
    public string? InheritDoc { get; set; }

    public virtual void AddDocumentation(XPathNavigator documentation)
    {
        switch (documentation.Name)
        {
            case "typeparam":
                var typeParamName = documentation.GetAttribute("name", "");
                var typeParamDesc = documentation.InnerXml.Trim();

                TypeParams.Add(typeParamName, typeParamDesc);
                break;

            case "param":
                var paramName = documentation.GetAttribute("name", "");
                var paramDesc = documentation.InnerXml.Trim();

                Params.Add(paramName, paramDesc);
                break;

            case "summary":
                Summary = documentation.InnerXml.Trim();
                break;

            case "remarks":
                Remarks = documentation.InnerXml.Trim();
                break;

            case "inheritdoc":
                InheritDoc = documentation.GetAttribute("cref", "");
                break;

            default:
                throw new NotImplementedException();
        }
    }
}