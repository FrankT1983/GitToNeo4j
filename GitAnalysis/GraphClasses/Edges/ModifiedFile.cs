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
}
