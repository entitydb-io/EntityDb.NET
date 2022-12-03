using System.Collections.Generic;

namespace EntityDb.DocumentationGenerator.Nodes
{
    public abstract class Node
    {
        public Dictionary<string, Node> ChildNodes { get; init; } = new();

        public bool HasXmlDocComment { get; set; }

        public abstract void Add(string path, Node node);
    }
}