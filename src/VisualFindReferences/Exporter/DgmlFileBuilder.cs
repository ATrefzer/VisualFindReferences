using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace VisualFindReferences.Exporter
{
    /// <summary>
    ///     Builder class to create a directed graph file to be processed with Visual Studio's 
    ///     DGML viewer.
    /// </summary>
    public class DgmlFileBuilder
    {
        private readonly Dictionary<string, Dictionary<string, string>> _categories;
        private readonly List<Edge> _edges;

        private readonly Dictionary<string, Node> _nodes;


        public DgmlFileBuilder()
        {
            _categories = new Dictionary<string, Dictionary<string, string>>();
            _edges = new List<Edge>();
            _nodes = new Dictionary<string, Node>();
        }

        /// <summary>
        ///     Adds a category, for example AddCategory("HotTemperature", "Background" "Red")
        /// </summary>
        public void AddCategory(string category, string property, string value)
        {
            if (!_categories.TryGetValue(category, out var properties))
            {
                properties = new Dictionary<string, string>();
                _categories.Add(category, properties);
            }

            properties[property] = value;
        }

        public Node AddNodeById(string nodeName, string id)
        {
            var node = new Node(id, nodeName);
            _nodes.Add(id, node);
            return node;
        }

        public Node AddNodeById(string nodeName, string id, string category)
        {
            var node = new Node(id, nodeName);
            node.Category = category;
            _nodes.Add(id, node);
            return node;
        }


        public void AddEdgeById(string sourceId, string targetId)
        {
            _edges.Add(new Edge(sourceId, targetId));
        }


        /// <summary>
        ///     Creates the output file.
        /// </summary>
        public void WriteOutput(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var writer = XmlWriter.Create(path))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("DirectedGraph", "http://schemas.microsoft.com/vs/2009/dgml");

                WriteCategories(writer);
                WriteNodes(writer);
                WriteEdges(writer);

                writer.WriteEndElement(); // DirectedGraph
                writer.WriteEndDocument();
            }
        }

        private void WriteEdges(XmlWriter writer)
        {
            writer.WriteStartElement("Links");
            foreach (var edge in _edges)
            {
                writer.WriteStartElement("Link");
                writer.WriteAttributeString("Source", edge.Source);
                writer.WriteAttributeString("Target", edge.Target);
                if (string.IsNullOrEmpty(edge.Category))
                {
                    writer.WriteAttributeString("Category", edge.Category);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void WriteNodes(XmlWriter writer)
        {
            writer.WriteStartElement("Nodes");
            foreach (var node in _nodes.Values)
            {
                var id = node.Id;
                writer.WriteStartElement("Node");
                writer.WriteAttributeString("Id", id);

                var escaped = node.Name;
                if (node.Name.Any(IsNonPrintableCharacter))
                {
                    escaped = "Cryptic_" + node.Name;
                }

                writer.WriteAttributeString("Label", escaped);

                if (node.HasCategory)
                {
                    writer.WriteAttributeString("Category", node.Category);
                }

                if (node.HasTooltip)
                {
                    writer.WriteAttributeString("Tooltip", node.Tooltip);
                }


                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void WriteCategories(XmlWriter writer)
        {
            writer.WriteStartElement("Categories");
            foreach (var category in _categories)
            {
                if (!category.Value.Any())
                {
                    continue;
                }

                writer.WriteStartElement("Category");
                writer.WriteAttributeString("Id", category.Key);
                foreach (var property in category.Value)
                {
                    writer.WriteAttributeString(property.Key, property.Value);
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private static bool IsNonPrintableCharacter(char candidate)
        {
            return candidate < 0x20 || candidate > 127;
        }

        public class Node
        {
            public Node(string id, string name)
            {
                Id = id;
                Name = name;
                Tooltip = "";
                Category = "";
            }

            public string Id { get; set; }
            public string Name { get; set; }
            public string Tooltip { get; set; }
            public string Category { get; set; }

            public bool HasCategory => !string.IsNullOrEmpty(Category);
            public bool HasTooltip => !string.IsNullOrEmpty(Tooltip);

            public Node WithTooltip(string tooltip)
            {
                Tooltip = tooltip;
                return this;
            }

            public Node WithCategory(string category)
            {
                Category = category;
                return this;
            }
        }

        internal class Edge
        {
            public Edge(string sourceNode, string targetNode)
            {
                Source = sourceNode;
                Target = targetNode;
                Category = "";
            }

            public Edge(string sourceNode, string targetNode, string category)
            {
                Source = sourceNode;
                Target = targetNode;
                Category = category;
            }

            public string Source { get; }
            public string Target { get; }
            public string Category { get; }
        }
    }
}