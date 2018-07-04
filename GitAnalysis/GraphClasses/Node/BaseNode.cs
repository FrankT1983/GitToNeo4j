namespace GitAnalysis.GraphClasses.Node
{
    public class BaseNode
    {
        public BaseNode(Neo4jWrapper neo4jwrapp)
        {            
            this.Id = neo4jwrapp != null ? neo4jwrapp.NextNodeId() : -1;
        }

        public BaseNode(long id)
        {
            this.Id = id;
        }
        public long Id { get; internal set; }
    }
}
