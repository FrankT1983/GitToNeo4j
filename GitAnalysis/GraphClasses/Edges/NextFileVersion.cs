namespace GitAnalysis.GraphClasses.Edges
{
    public class NextFileVersion : BaseEdge
    {    
        public NextFileVersion(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public NextFileVersion() : base(-1)
        {
        }
    }
}
