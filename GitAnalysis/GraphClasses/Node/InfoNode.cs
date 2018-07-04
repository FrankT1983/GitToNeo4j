using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitAnalysis.GraphClasses.Node
{

    public class InfoNode : BaseNode
    {
        public InfoNode(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public InfoNode(long id) : base(id)
        {
        }

        public InfoNode() : base(-1)
        {

        }

        public string CloneFolder { get; internal set; }
        public string AnalysisFolder { get; internal set; }
        public int NextNodeId { get; internal set; }
        public int NextEdgeId { get; internal set; }        
    }
}
