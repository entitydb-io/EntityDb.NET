using EntityDb.DocumentationGenerator.Nodes;

namespace EntityDb.DocumentationGenerator.Services.PrintingService;

public class ConsolePrintingService : IPrintingService
{
    public void Print(NamespaceNode namespaceNode)
    {
        Print(0, "", namespaceNode);
    }

    private void Print(int depth, string path, Node node)
    {
        var prefix = string.Join("", Enumerable.Repeat(" ", depth));

        Console.WriteLine($"{prefix}{path}<{node}>");

        if (node.Summary != null)
        {
            Console.WriteLine($"{prefix}- Summary: {node.Summary}");
        }

        if (node.Remarks != null)
        {
            Console.WriteLine($"{prefix}- Remarks: {node.Remarks}");
        }

        if (node is MemberInfoNode memberInfoNode)
        {
            if (memberInfoNode.TypeParams.Count > 0)
            {
                Console.WriteLine($"{prefix}- TypeParams");

                foreach (var (typeParamName, typeParamDesc) in memberInfoNode.TypeParams)
                {
                    Console.WriteLine($"{prefix}  - {typeParamName}: {typeParamDesc}");
                }
            }

            if (memberInfoNode.Params.Count > 0)
            {
                Console.WriteLine($"{prefix}- Params");

                foreach (var (paramName, paramDesc) in memberInfoNode.Params)
                {
                    Console.WriteLine($"{prefix}  - {paramName}: {paramDesc}");
                }
            }
        }

        Console.WriteLine("");

        foreach (var (childPath, childNode) in node.ChildNodes)
        {
            Print(depth + 1, childPath, childNode);
        }
    }
}
