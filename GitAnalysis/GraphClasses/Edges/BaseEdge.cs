namespace GitAnalysis.GraphClasses.Edges
{
    public class BaseEdge
    {
        public BaseEdge(Neo4jWrapper neo4jwrapp)
        {
            this.Id = neo4jwrapp.NextEdgeId();
        }

        public BaseEdge(long id)
        {
            this.Id = id;
        }

        public BaseEdge()
        {
            this.Id = -1;
        }
        public long Id { get; internal set; }
    }
}
