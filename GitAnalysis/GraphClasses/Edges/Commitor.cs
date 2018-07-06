namespace GitAnalysis.GraphClasses.Edges
{
    public class Commitor : BaseEdge
    {
        public Commitor(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public Commitor() : base(-1)
        {
        }
    }
}
