using GitAnalysis.GraphClasses.Edges;
using GitAnalysis.GraphClasses.Node;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitAnalysis
{
    public class AnalysisBase
    {
        public string RepoCloneFolder { get; }
        public string AnalysisDestination { get; }
        public Repository Repo { get; }

        public Neo4jWrapper neo4jwrapp = new Neo4jWrapper();

        public AnalysisBase(string repoCloneFolder, string analysisDestinatinFolder)
        {
            this.RepoCloneFolder = repoCloneFolder;
            this.AnalysisDestination = analysisDestinatinFolder + "\\";
            this.Repo = new Repository(this.RepoCloneFolder);
            this.neo4jwrapp.Connect();

            this.neo4jwrapp.WriteOrUpdateInfoNode(this.RepoCloneFolder,this.AnalysisDestination);            
        }

        public void WriteCommitNodes()
        {            
            List<LibGit2Sharp.Commit> commits = CommitsByTime();

            AddPersonsToGraph(commits);
            AddCommitsAndLinkToPersons(commits);
            //AddCommitParentChildDependencies(result, commits);
            //AddCommitFiles(result, commits);
        }

        private List<LibGit2Sharp.Commit> CommitsByTime()
        {
            var commits = new List<LibGit2Sharp.Commit>(Repo.Commits);
            commits.Sort((a, b) => a.Author.When.CompareTo(b.Author.When));
            return commits;
        }

        private void AddPersonsToGraph(List<LibGit2Sharp.Commit> commits)
        {
            foreach (var a in commits.Select(c => c.Author))
            {
                {
                    var searchPerson = new Person() { Name = a.Name, EMail = a.Email };
                    if (neo4jwrapp.Find(searchPerson).Any())
                    {
                        continue;
                    }
                }

                var personNextId = new Person(neo4jwrapp) { Name = a.Name, EMail = a.Email }; 
                this.neo4jwrapp.WriteNode(personNextId);
                this.neo4jwrapp.UpdateInfo(Neo4jWrapper.GraphStatus.WrotePeople);                               
            }
        }

        public void Clear()
        {
            this.neo4jwrapp.ClearDB();
        }

        private void AddCommitsAndLinkToPersons(List<LibGit2Sharp.Commit> commits)
        {
            foreach (var c in commits)
            {
                {
                    var searchCommit = new GraphClasses.Node.Commit() { Sha = c.Sha, Message = c.Message };
                    if (neo4jwrapp.Find<BaseNode>(searchCommit).Any())
                    {
                        continue;
                    }
                }

                var commitNode = new GraphClasses.Node.Commit(this.neo4jwrapp) { Sha = c.Sha, Message = c.Message };
                this.neo4jwrapp.WriteNode(commitNode);

                this.neo4jwrapp.WriteEdge(new Person(null) { Name = c.Author.Name, EMail = c.Author.Email }, commitNode, new Author(this.neo4jwrapp));



                
                //result.AddEdge(new Edge<long>(result.NextEdgeId(), result.FindPersonNode(c.Committer), commitNode.Id, EdgeTypeHelpers.CommiterEdgeAttributes()));
            }
        }
    }
}
