namespace GitAnalysis.GraphClasses.Node
{
    public class BaseNode
    {
        internal static string ContainsFile = "ContainsFile";

        public BaseNode(Neo4jWrapper neo4jwrapp)
        {            
            this.Id = neo4jwrapp != null ? neo4jwrapp.NextNodeId() : -1;
        }

        public BaseNode(long id)
        {
            this.Id = id;
        }


        public BaseNode()
        {
            int shouldNotHappen = 0;
        }

        public long Id { get; internal set; }
    }
}
