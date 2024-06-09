using System.Collections.Generic;
using System.Linq;
using VisualFindReferences.Core.Graph.Model;
using VisualFindReferences.Core.Graph.Model.Nodes;

namespace VisualFindReferences.Exporter
{
    internal class DgmlExport
    {
        // Used categories
        const string Method = "method";
        const string Class = "class";
        const string Other = "other";

        internal static void Export(string fileName, IEnumerable<Node> nodes, IEnumerable<Connector> edges)
        {
            var writer = new DgmlFileBuilder();

            WriteCategories(writer);
            WriteNodes(writer, nodes);
            WriteEdges(writer, edges);

            writer.WriteOutput(fileName);
        }

        private static void WriteCategories(DgmlFileBuilder writer)
        {
            writer.AddCategory(Method, "Background", "#ffdcdcdc");
            writer.AddCategory(Class, "Background", "#ff808080"); 
            writer.AddCategory(Other, "Background", "#ff404040");
        }

        private static void WriteEdges(DgmlFileBuilder writer, IEnumerable<Connector> edges)
        {
            foreach (var edge in edges)
            {
                writer.AddEdgeById(edge.StartNode.Id, edge.EndNode.Id);
            }
        }

        private static void WriteNodes(DgmlFileBuilder writer, IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (node is MethodNode)
                {
                    writer.AddNodeById(GetDgmlLabel(node), node.Id, Method);
                }
                else if (node is ClassNode)
                {
                    writer.AddNodeById(GetDgmlLabel(node), node.Id, Class);
                }
                else
                {
                    writer.AddNodeById(GetDgmlLabel(node), node.Id, Other);
                }
            }
        }

        private static string GetDgmlLabel(Node node)
        {
            var parts = new List<string> { node.TypeName, node.ContainerName }.Where(x => !string.IsNullOrEmpty(x));
            return string.Join(".", parts);
        }
    }
}
