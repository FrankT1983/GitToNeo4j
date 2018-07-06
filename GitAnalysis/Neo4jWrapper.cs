using GitAnalysis.GraphClasses.Edges;
using GitAnalysis.GraphClasses.Node;
using Neo4j.Driver.V1;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitAnalysis
{
    public partial class Neo4jWrapper
    {
        public enum GraphStatus
        {
            WrotePeople,
            WroteCommits,

        }

        private long nextNodeId = 0;
        private long nextEdgeId = 0;

        public long NextNodeId()
        {
            var next = ++nextNodeId;
            UpdateInfo();
            return next;
        }

        public long NextEdgeId()
        {
            var next = ++nextEdgeId;
            UpdateInfo();
            return next;
        }

        GraphClient client;
        internal void Connect()
        {
            this.client = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "password");
            this.client.Connect();
        }

        internal InfoNode GetInfoNode()
        {
            InfoNode existing = this.client.Cypher.Match("(n:" + typeof(InfoNode).Name + ")").Return<InfoNode>("n").Results.FirstOrDefault();
            return existing;

        }

        internal InfoNode WriteOrUpdateInfoNode(string repoCloneFolder, string analysisDestination)
        {
            var info = this.GetInfoNode();
            if (info == null)
            {
                var n = new InfoNode(1) { CloneFolder = repoCloneFolder, AnalysisFolder = repoCloneFolder, NextNodeId = 2, NextEdgeId = 1 };
                WriteNode(n);
                info = this.GetInfoNode();
            }

            this.nextNodeId = info.NextNodeId;
            this.nextEdgeId = info.NextEdgeId;
            return info;
        }

        internal void WriteNode(BaseNode obj)
        {
            client.Cypher.Create("(n:" + obj.GetType().Name + " {newObject})")
               .WithParam("newObject", obj)
               .ExecuteWithoutResults();
        }


        private void UpdateNode(InfoNode node)
        {
            client.Cypher.MatchQuerry("n", node)
                    .Set("n.NextNodeId = {NextNodeId}")
                    .WithParam("NextNodeId", node.NextNodeId)
                    .Set("n.NextEdgeId = {NextEdgeId}")
                    .WithParam("NextEdgeId", node.NextEdgeId)
               .ExecuteWithoutResults();
        }

        internal void UpdateInfo()
        {
            var info = this.GetInfoNode();
            info.NextEdgeId = this.nextEdgeId;
            info.NextNodeId = this.nextNodeId;

            this.UpdateNode(info);
        }

        internal IEnumerable<T> Find<T>(T node) where T : BaseNode
        {
            var query = client.Cypher.MatchQuerry("n", node).Return<T>("n");
            return query.Results;
        }

        internal void UpdateInfo(GraphStatus wrotePeople)
        {

        }

        internal void ClearDB()
        {
            //"MATCH (n) OPTIONAL MATCH(n)-[r]-() DELETE n,r"
            this.client.Cypher.Match("()-[r]-()").Delete("r").ExecuteWithoutResults();
            this.client.Cypher.Match("(n)").Delete("n").ExecuteWithoutResults();            
        }

        internal void WriteEdge(BaseNode fromNode, BaseNode toNode, BaseEdge edge)
        {                    
            var query = client.Cypher.MatchQuerry("n", fromNode)
                         .MatchQuerry("m", toNode).
                             Create("(n)-[:"+ edge.GetType().Name+ " {newEdge}]->(m)").
                             WithParam("newEdge", edge);

            query.ExecuteWithoutResults();
        }

        internal IEnumerable<T> Find<T>(Type type)
        {
            var query = client.Cypher.Match("(n:" + type.Name + ")").Return<T>("n");
            return query.Results;
        }

        internal IEnumerable<T> FindWithoutLabel<T>(Type type, string label)
        {
            var query = client.Cypher.Match("(n:" + type.Name + ")")
                .Where("not n:" + label)
                .Return<T>("n");
            return query.Results;
        }

        internal void AddLabel(File file, string hasAstLabel)
        {
            var query = client.Cypher.MatchQuerry("n", file).Set("n:" + hasAstLabel );
            query.ExecuteWithoutResults();
        }
       
        internal IEnumerable<Neo4jEdge<N1,E,N2>> FindAllEdgesBetweenTypes<N1, E, N2>()
        {
            //match (:Commit)-[e:ModifiedFile]->(n:File) return e.ChangeParrentCommit , n
            var query = client.Cypher.Match("(sourceNode:"+ typeof(N1).Name  + " )-[edge:"+ typeof(E).Name + "]->(destinationNode:" + typeof(N2).Name  +")")
                .Return<Neo4jEdge<N1, E, N2>>((sourceNode, edge, destinationNode) => new Neo4jEdge<N1, E, N2>()
                {
                    From = sourceNode.As<N1>(),
                    To = destinationNode.As<N2>(),
                    Edge = edge.As<E>()
                });
            return  query.Results;            
        }
    }
}