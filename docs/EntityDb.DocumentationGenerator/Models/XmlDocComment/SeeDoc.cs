﻿using System.Xml;
using System.Xml.Serialization;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class SeeDoc
{
    [XmlAttribute("cref")]
    public required string SeeRef { get; init; }
}
