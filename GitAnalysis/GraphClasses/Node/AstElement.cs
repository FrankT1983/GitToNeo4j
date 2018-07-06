using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitAnalysis.GraphClasses.Node
{
    public class AstElement : BaseNode
    {
        public long Start { get; internal set; }
        public long End { get; internal set; }
        public string Type { get; internal set; }
        public long AstId { get; internal set; }
        public string FilePath { get; internal set; }
        public string CommitSha { get; internal set; }

        public AstElement(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public AstElement(long id) : base(id)
        {
        }

        public AstElement() : base(-1)
        {

        }
    }
}
