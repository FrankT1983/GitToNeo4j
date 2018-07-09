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
}
