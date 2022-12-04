using System.Xml.Serialization;
using System.Xml;
using System.Text;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public abstract class DocWithMixedInnerXml
{
    [XmlText(typeof(XmlText))]
    [XmlElement("see", typeof(SeeDoc))]
    [XmlElement("paramref", typeof(ParamRefDoc))]
    [XmlElement("typeparamref", typeof(TypeParamRefDoc))]
    [XmlElement("c", typeof(CodeDoc))]
    [XmlAnyElement]
    public required object[] Elements { get; init; }

    //TODO: Need to pass some interface that can format the objects
    public string GetText()
    {
        var stringBuilder = new StringBuilder();

        foreach (var element in Elements)
        {
            switch (element)
            {
                case XmlText xmlText:
                    stringBuilder.Append(xmlText.OuterXml);
                    break;

                case SeeDoc see:
                    stringBuilder.Append($"<<see:{see.SeeRef}>>");
                    break;

                case CodeDoc code:
                    stringBuilder.Append($"<<code:{code.GetText()}>>");
                    break;

                case ParamRefDoc paramRef:
                    stringBuilder.Append($"<<paramref:{paramRef.Name}>>");
                    break;

                case TypeParamRefDoc typeParamRef:
                    stringBuilder.Append($"<<typeparamref:{typeParamRef.Name}>>");
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        return stringBuilder.ToString();
    }
}
