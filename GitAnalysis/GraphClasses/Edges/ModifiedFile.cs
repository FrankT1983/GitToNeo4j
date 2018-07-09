namespace GitAnalysis.GraphClasses.Edges
{
    public class ModifiedFile : BaseEdge
    {
        public string ChangeParrentCommit { get; internal set; }

        public ModifiedFile(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public ModifiedFile() : base(-1)
        {
        }
    }

    public class ContainsFile : BaseEdge
    {
        public string ChangeParrentCommit { get; internal set; }

        public ContainsFile(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public ContainsFile() : base(-1)
        {
        }
    }
}
