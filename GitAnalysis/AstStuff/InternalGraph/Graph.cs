using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitAnalysis.GraphClasses.Node;

namespace GitAnalysis.AstStuff.InternalGraph
{
    public class AstGraph
    {
        private List<AstElement> nodes = new List<AstElement>();
        private List<Edge> edges = new List<Edge>();

        public IEnumerable<AstElement> Nodes { get { return nodes.AsReadOnly(); }  }

        public IEnumerable<Edge> Edges {get  { return edges.AsReadOnly(); } }

        internal void AddNode(AstElement node)
        {
            this.nodes.Add(node);
        }

        internal void AddEdge(Edge edge)
        {
            this.edges.Add(edge);
        }

        internal AstElement GetRoot()
        {
            foreach(var n in this.Nodes)
            {
                if (this.edges.Any(e => e.To == n.AstId)) { continue; }

                return n;
            }
            throw new EntryPointNotFoundException("Did not find root node");            
        }
    }

    [DebuggerDisplay("{From} -> {To}")]
    public class Edge
    {
        public long From { get; set; }
        public long To { get; set; }
    }

    [DebuggerDisplay("{From} -[{Mode}]> {To}")]
    public class TransitionEdge
    {
        public long From { get; set; }
        public long To { get; set; }
        public String Mode { get; set; }
    }
}
