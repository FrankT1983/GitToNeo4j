namespace GitAnalysis.GraphClasses.Edges
{
    public class CodeRemoved: BaseEdge
    {
        public CodeRemoved(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public CodeRemoved() : base(-1)
        {
        }
    }

}
