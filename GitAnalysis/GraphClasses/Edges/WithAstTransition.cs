namespace GitAnalysis.GraphClasses.Edges
{
    public class WithAstTransition : BaseEdge
    {
        public WithAstTransition(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public WithAstTransition() : base(-1)
        {
        }
    }
}
