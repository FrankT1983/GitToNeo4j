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

    public class CreatedFile : BaseEdge
    {
        public CreatedFile(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public CreatedFile() : base(-1)
        {
        }
    }

    public class RenamedFile : BaseEdge
    {
        public RenamedFile(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public RenamedFile() : base(-1)
        {
        }
    }

    public class ModifiedFile : BaseEdge
    {
        public ModifiedFile(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public ModifiedFile() : base(-1)
        {
        }
    }


    public class Commitor : BaseEdge
    {
        public Commitor(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public Commitor() : base(-1)
        {
        }
    }

    public class CommitParent : BaseEdge
    {
        public CommitParent(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public CommitParent() : base(-1)
        {
        }
    }
}
