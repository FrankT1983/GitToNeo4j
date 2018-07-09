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

        internal void WriteEdge(BaseNode fromNode, BaseNode toNode, BaseEdge edge, BaseEdge edge2)
        {
            var query = client.Cypher.MatchQuerry("n", fromNode)
                         .MatchQuerry("m", toNode).
                             Create("(n)-[:" + edge.GetType().Name + " {newEdge}]->(m)").
                             Create("(n)-[:" + edge2.GetType().Name + " {newEdge2}]->(m)").
                             WithParam("newEdge", edge).WithParam("newEdge2", edge2);

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

        internal void AddLabel(BaseNode file, string hasAstLabel)
        {
            var query = client.Cypher.MatchQuerry("n", file).Set("n:" + hasAstLabel );
            query.ExecuteWithoutResults();
        }


        internal IEnumerable<Neo4jEdge<N1, E, N2>> FindAllEdgesBetweenNodes<N1, E, N2>(N1 n1, N2 n2) where N1 : BaseNode where N2: BaseNode
        {
            //match (:Commit)-[e:ModifiedFile]->(n:File) return e.ChangeParrentCommit , n
            var query = client.Cypher.Match("(sourceNode:" + typeof(N1).Name + " )-[edge:" + typeof(E).Name + "]->(destinationNode:" + typeof(N2).Name + ")")
                .MatchQuerry("sourceNode", n1)
                .MatchQuerry("destinationNode",n2)
                .Return<Neo4jEdge<N1, E, N2>>((sourceNode, edge, destinationNode) => new Neo4jEdge<N1, E, N2>()
                {
                    From = sourceNode.As<N1>(),
                    To = destinationNode.As<N2>(),
                    Edge = edge.As<E>()
                });
            return query.Results;
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

        internal void LinkFileBetweenCommits()
        {
            // todo: not sure, if I can seperate the finding of this convoluted chain good enough.
            // scr:File <[ContainsFile]-Commit-[nextCommit]->Commit-[ContainsFile]->dst:File mit pfad == 
            string fileNode = typeof(File).Name;
            string commitNode = "(:" + typeof(Commit).Name + ")";
            string fileEdge = typeof(ContainsFile).Name ;
            string nextCommitEdge = typeof(NextCommit).Name;
            var query = client.Cypher.Match("(scr:" + fileNode + ")<-" + "[e1:" + fileEdge + "]-" + commitNode + "-["+ nextCommitEdge + "]->" + commitNode + "-[e2:" + fileEdge + "]->(dst:" + fileNode + ")")
                .Where("scr.Path = dst.Path")
                .Create(" (scr)-[:"+typeof(NextFileVersion).Name+ "]->(dst)")                
                .Delete("e1,e2");

            var tmp = query.Query.DebugQueryText;

            query.ExecuteWithoutResults();
        }

        internal void RemoveIntermediateNoChanges()
        {
            // shrink multiple non modifications
            var query = this.client.Cypher.Match("(f1)-[e1:NextFileVersion]->(f2)-[e2:NextFileVersion]->(f3),(c1)-[e3:NoModification]->(f2)")
                .Delete("f2,e1,e2,e3")
                .Create(" (f1)-[:" + typeof(NextFileVersion).Name + "]->(f3)");
             query.ExecuteWithoutResults();

            // remove last bit
            // Match : match (f1)-[e1:NextFileVersion]->(f2)-[e2:NextFileVersion]->(f3),(c)-[e3:NoModification]->(f2) return f2
            // create f1->f3
            // remove e1,e2,e3,f2            
        }

        internal void PropagadeModifictionAttribute()
        {
            // "match (n1:AstElement)-[:ModifiedCode]->(n2:AstElement), (above:AstElement)-[:AstAbove*]->(n1) set above.ChildWasModified = above.ChildWasModified +1"
            this.client.Cypher.Match("(n1:AstElement)-[:ModifiedCode]->(n2:AstElement), (above:AstElement)-[:AstAbove*]->(n1)")
                .Set("above.ChildWasModified = above.ChildWasModified +1")
                .ExecuteWithoutResults();
        }

        internal void AddTransitionEdge(string fromCommitSha, string fromCommitPath, long fromAstId, string toCommitSha, string toCommitPaht, long toAstId, BaseEdge edge)
        {
            var query = this.client.Cypher                        
                        .MatchQuerry("n1", new AstElement() { AstId = fromAstId, FilePath = fromCommitPath, CommitSha = fromCommitSha })
                        .MatchQuerry("n2", new AstElement() { AstId = toAstId, FilePath = toCommitPaht, CommitSha = toCommitSha })
                        .Create("(n1)-[:" + edge.GetType().Name + " {newEdge}]->(n2)")
                        .WithParam("newEdge", edge);

            query.ExecuteWithoutResults();
        }

        internal void AddTransitionEdge(string fromCommitSha, string fromCommitPath, long fromAstId, BaseNode destNode, BaseEdge edge)
        {
            var query = this.client.Cypher
                        .MatchQuerry("n1", new AstElement() { AstId = fromAstId, FilePath = fromCommitPath, CommitSha = fromCommitSha })
                        .MatchQuerry("n2", destNode)
                        .Create("(n1)-[:" + edge.GetType().Name + " {newEdge}]->(n2)")
                        .WithParam("newEdge", edge);

            query.ExecuteWithoutResults();
        }
    }
}