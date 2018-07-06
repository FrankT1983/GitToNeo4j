namespace GitAnalysis.GraphClasses.Edges
{
    public class AstAbove : BaseEdge
    {
        public AstAbove(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public AstAbove() : base(-1)
        {
        }
    }

    public class AstOfFile : BaseEdge
    {
        public AstOfFile(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public AstOfFile() : base(-1)
        {
        }
    }
}
