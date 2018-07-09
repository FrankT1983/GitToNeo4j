namespace GitAnalysis.GraphClasses.Edges
{
    public class AstOfFile : BaseEdge
    {
        public AstOfFile(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public AstOfFile() : base(-1)
        {
        }
    }

    public class AstSpecialNode : BaseEdge
    {
        public AstSpecialNode(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public AstSpecialNode() : base(-1)
        {
        }
    }
}
