namespace GitAnalysis.GraphClasses.Edges
{
    public class RenamedFile : BaseEdge
    {
        public RenamedFile(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public RenamedFile() : base(-1)
        {
        }
    }
}
