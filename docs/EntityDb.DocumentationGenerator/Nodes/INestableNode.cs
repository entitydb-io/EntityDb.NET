namespace EntityDb.DocumentationGenerator.Nodes;

public interface INestableNode
{
    public abstract void AddChild(string path, INode node);

    public abstract IEnumerable<KeyValuePair<string, INode>> GetChildNodes();
}
