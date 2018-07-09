namespace GitAnalysis.GraphClasses.Edges
{
    public class NoModification : BaseEdge
    {       
        public NoModification(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public NoModification() : base(-1)
        {
        }
    }
}
