using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Microsoft.CodeAnalysis;

namespace VisualFindReferences.Core.Graph.Model.Nodes
{
    public abstract class VFRNode : Node
    {
        private bool _noMoreReferences;

        private bool _referenceLocationsAdded;

        protected VFRNode(NodeGraph flowChart, FoundReferences foundReferences, Geometry icon, Brush iconColor) : base(
            flowChart, GetContainerName(foundReferences), GetTypeName(foundReferences), 0, 0, icon, iconColor)
        {
            NodeFoundReferences = foundReferences;
            NamespaceName = foundReferences.Symbol.ContainingNamespace.ToDisplayString();
            AssemblyName = foundReferences.Symbol.ContainingAssembly.Name;
        }

        public bool ShouldShowTypeName => !string.IsNullOrWhiteSpace(TypeName);

        public string NamespaceName { get; }

        public string AssemblyName { get; }

        public abstract string NodeSymbolType { get; }

        public FoundReferences NodeFoundReferences { get; }

        // only set for the root node
        public Document? SourceDocument { get; set; }

        public List<ISymbol> SearchedSymbols { get; } = new List<ISymbol>();

        public bool NoMoreReferences
        {
            get => _noMoreReferences;
            set
            {
                if (value != _noMoreReferences)
                {
                    _noMoreReferences = value;
                    RaisePropertyChanged(nameof(NoMoreReferences));
                }
            }
        }

        public bool ReferenceLocationsAdded
        {
            get => _referenceLocationsAdded;
            set
            {
                if (value != _referenceLocationsAdded)
                {
                    _referenceLocationsAdded = value;
                    RaisePropertyChanged(nameof(ReferenceLocationsAdded));
                }
            }
        }

        public string FullName
        {
            get
            {
                var parts = new List<string>
                {
                    NamespaceName, TypeName, ContainerName
                };
                var name = string.Join(".", parts.Where(x => string.IsNullOrEmpty(x) is false));
                return name;
            }
        }

        private static string GetContainerName(FoundReferences foundReferences)
        {
            var name = foundReferences.Symbol.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Unnamed";
            }

            return name;
        }

        private static string GetTypeName(FoundReferences foundReferences)
        {
            if (foundReferences.Symbol.ContainingType is ITypeSymbol typeSymbol)
            {
                return typeSymbol.Name;
            }

            return string.Empty;
        }

        public abstract IEnumerable<SearchableSymbol> GetSearchableSymbols();
    }
}