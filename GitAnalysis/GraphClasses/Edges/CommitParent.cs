namespace GitAnalysis.GraphClasses.Edges
{
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
