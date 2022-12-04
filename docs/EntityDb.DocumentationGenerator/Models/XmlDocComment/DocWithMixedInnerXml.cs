using System.Xml.Serialization;
using System.Xml;
using System.Text;
using EntityDb.DocumentationGenerator.Services.PrintingService;

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

    public string GetText(IPrintingService printingService)
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
                    stringBuilder.Append(printingService.ConvertSeeDoc(see));
                    break;

                case CodeDoc code:
                    stringBuilder.Append(printingService.ConvertCodeDoc(code));
                    break;

                case ParamRefDoc paramRef:
                    stringBuilder.Append(printingService.ConvertParamRefDoc(paramRef));
                    break;

                case TypeParamRefDoc typeParamRef:
                    stringBuilder.Append(printingService.ConvertTypeParamRefDoc(typeParamRef));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        return stringBuilder.ToString();
    }
}
