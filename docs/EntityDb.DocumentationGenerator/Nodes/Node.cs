using System.Xml.XPath;

namespace EntityDb.DocumentationGenerator.Nodes;

public abstract class Node
{
    public string? Summary { get; set; }
    public string? Remarks { get; set; }
    public string? InheritDoc { get; set; }

    public virtual void AddDocumentation(XPathNavigator documentation)
    {
        switch (documentation.Name)
        {
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
                break;
        }
    }
}