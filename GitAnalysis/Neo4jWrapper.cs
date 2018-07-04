using GitAnalysis.GraphClasses.Edges;
using GitAnalysis.GraphClasses.Node;
using Neo4j.Driver.V1;
using Neo4jClient;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitAnalysis
{
    public class Neo4jWrapper
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
            return nextNodeId++;
        }

        public long NextEdgeId()
        {
            return nextEdgeId++;
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
                var n = new InfoNode(0) { CloneFolder = repoCloneFolder, AnalysisFolder = repoCloneFolder, NextNodeId = 1, NextEdgeId = 0 };
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

        internal IEnumerable<T> Find<T>(T node) where T : BaseNode
        {
            return client.Cypher.MatchQuerry("n", node).Return<T>("n").Results;
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

        internal void WriteEdge(Person fromNode, BaseNode toNode, BaseEdge edge)
        {
            if (toNode.Id == -1)
            {
                throw new NotImplementedException();
            }

            var tmp = client.Cypher.MatchQuerry("n", fromNode).MatchQuerry("m", toNode).Query.DebugQueryText;

            client.Cypher.MatchQuerry("n", fromNode)
                         .MatchQuerry("m", toNode).
                             Create("(n)-[:"+ edge.GetType().Name+ " {newEdge}]->(m)").
                             WithParam("newEdge", edge).ExecuteWithoutResults();

            //client.Cypher.MatchQuerry("n", fromNode).MatchQuerry("m", toNode).
            //                 Create("(n)-[:" + edge.GetType().Name + "}]->(m)");

            //client.Cypher.MatchQuerry("n", fromNode).MatchQuerry("m", toNode).Create("(n)-[:" + edge.GetType().Name + "}]->(m)").ExecuteWithoutResults();

            var tmp2 = client.Cypher.MatchQuerry("n", fromNode).MatchQuerry("m", toNode).Create("(n)-[:" + edge.GetType().Name + "}]->(m)").Query.DebugQueryText;
        }
       
    }

    public static class CypherFluentExtension
    {     
        public static ICypherFluentQuery MatchQuerry(this ICypherFluentQuery cypher, string varialbeName, BaseNode node)
        {
            if (node is Person)
            {               
              return cypher.MatchQuerry(varialbeName, (Person)node);
            }
            if (node is Commit)
            {
                return cypher.MatchQuerry(varialbeName, (Commit)node);
            }

            string matchClause = "(" + varialbeName + ":" + node.GetType().Name + ")";
            if (node.Id > 0)
            {
                return cypher.Match(matchClause).Where(varialbeName + ".Id=" + node.Id);
            }

            throw new NotImplementedException("Needs a specialices MatchQuery for " + node.GetType().Name);
        }
        
        public static ICypherFluentQuery MatchQuerry(this ICypherFluentQuery cypher, string varialbeName, Person node)
        {
            string matchClause = "(" + varialbeName + ":" + node.GetType().Name + ")";           
            string whereClause = varialbeName + "." + nameof(node.Name) + "=\"" + node.Name +"\""+
                                        " and  " + 
                                     varialbeName + "." + nameof(node.EMail) + "=\"" + node.EMail + "\"";
            return cypher.Match(matchClause).Where(whereClause);           
        }

        public static ICypherFluentQuery MatchQuerry(this ICypherFluentQuery cypher, string varialbeName, Commit node)
        {
            string matchClause = "(" + varialbeName + ":" + node.GetType().Name + ")";
            string whereClause = varialbeName + "." + nameof(node.Sha) + "=\"" + node.Sha + "\"";                                        
            return cypher.Match(matchClause).Where(whereClause);          
        }
    }
}