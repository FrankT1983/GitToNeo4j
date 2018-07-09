namespace GitAnalysis.GraphClasses.Node
{
    public class AstElementDeleted : BaseNode
    {
        public AstElementDeleted(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public AstElementDeleted(long id) : base(id)
        {
        }

        public AstElementDeleted() : base(-1)
        {

        }

        public string FilePath { get; internal set; }
        public string CommitSha { get; internal set; }
    }
}
