using GitAnalysis.GraphClasses.Edges;
using GitAnalysis.GraphClasses.Node;

namespace GitAnalysis
{
    public partial class Neo4jWrapper
    {
        public class Neo4jEdge<N1,E,N2>
        {
            public N1 From { get; internal set; }
            public N2 To { get; internal set; }
            public E Edge { get; internal set; }
        }
    }
}