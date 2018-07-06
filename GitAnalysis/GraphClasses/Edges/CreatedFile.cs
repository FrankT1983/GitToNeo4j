namespace GitAnalysis.GraphClasses.Edges
{
    public class CreatedFile : BaseEdge
    {
        public CreatedFile(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public CreatedFile() : base(-1)
        {
        }
    }
}
