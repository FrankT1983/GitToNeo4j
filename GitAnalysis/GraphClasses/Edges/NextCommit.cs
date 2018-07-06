namespace GitAnalysis.GraphClasses.Edges
{
    public class NextCommit : BaseEdge
    {
        public NextCommit(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public NextCommit() : base(-1)
        {
        }
    }
}
