namespace GitAnalysis.GraphClasses.Node
{
    public class Commit : BaseNode
    {
        public Commit(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public Commit() : base(-1)
        {
        }

        public string Sha { get; internal set; }
        public string Message { get; internal set; }
    }
}
