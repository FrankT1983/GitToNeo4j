using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitAnalysis.GraphClasses.Edges
{
    public class BaseEdge
    {
        public BaseEdge(Neo4jWrapper neo4jwrapp)
        {
            this.Id = neo4jwrapp.NextEdgeId();
        }

        public BaseEdge(long id)
        {
            this.Id = id;
        }
        public long Id { get; internal set; }
    }
    public class Author : BaseEdge
    {
        public Author(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public Author() : base(-1)
        {
        }
    }
}
