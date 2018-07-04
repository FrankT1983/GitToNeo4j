namespace GitAnalysis.GraphClasses.Node
{
    public class File : BaseNode
    {
        public string Path { get; internal set; }
        public string Commit { get; internal set; }

        public File(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public File() : base(-1)
        {

        }

     
    }

}
