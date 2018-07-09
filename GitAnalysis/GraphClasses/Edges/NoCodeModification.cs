namespace GitAnalysis.GraphClasses.Edges
{
    public class NoCodeModification : BaseEdge
    {
        public NoCodeModification(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public NoCodeModification() : base(-1)
        {
        }
    }

}
