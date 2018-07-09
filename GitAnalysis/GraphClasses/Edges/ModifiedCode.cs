namespace GitAnalysis.GraphClasses.Edges
{
    public class ModifiedCode : BaseEdge
    {
        public ModifiedCode(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public ModifiedCode() : base(-1)
        {
        }
    }

}
