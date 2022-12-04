﻿using System.Xml;
using System.Xml.Serialization;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class InheritDoc
{
    [XmlAttribute("cref")]
    public string? SeeRef { get; init; }
}
