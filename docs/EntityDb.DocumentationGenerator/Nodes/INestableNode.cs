namespace EntityDb.DocumentationGenerator.Nodes;

public interface INestableNode
{
    public abstract void AddChild(string path, Node node);

    public abstract IEnumerable<KeyValuePair<string, Node>> GetChildNodes();
}
