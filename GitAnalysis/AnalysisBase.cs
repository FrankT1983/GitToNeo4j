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

            this.neo4jwrapp.WriteOrUpdateInfoNode(this.RepoCloneFolder, this.AnalysisDestination);
        }

        public void WriteCommitNodes()
        {
            List<LibGit2Sharp.Commit> commits = CommitsByTime();

            AddPersonsToGraph(commits);
            AddCommitsAndLinkToPersons(commits);
            AddCommitParentChildDependencies(commits);
            AddCommitFiles(commits);
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
                this.neo4jwrapp.WriteEdge(new Person(null) { Name = c.Author.Name, EMail = c.Author.Email }, commitNode, new Commitor(this.neo4jwrapp));
            }
        }


        private void AddCommitParentChildDependencies(List<LibGit2Sharp.Commit> commits)
        {
            foreach (var c in commits)
            {
                foreach (var p in c.Parents)
                {
                    this.neo4jwrapp.WriteEdge(new GraphClasses.Node.Commit(null) { Sha = c.Sha },
                                              new GraphClasses.Node.Commit(null) { Sha = p.Sha },
                                              new CommitParent(this.neo4jwrapp));
                }
            }
        }

        private void AddCommitFiles(List<LibGit2Sharp.Commit> commits)
        {

            int i = 0;
            double count = commits.Count();
            foreach (var c in commits)
            {
                //FireProgressUpdate(i++ / count);                            
                if (c.Parents.Any())
                {                    
                    foreach (var p in c.Parents)
                    {
                      
                        foreach (var change in Repo.Diff.Compare<TreeChanges>(p.Tree, c.Tree))
                        {
                            // generate for changed file in current commit 
                            if (change.Exists)
                            {
                                {
                                    var fileSearch = new File() { Path = change.Path, Commit = c.Sha };
                                    if (neo4jwrapp.Find<BaseNode>(fileSearch).Any())
                                    {
                                        continue;
                                    }
                                }


                                var fileNode = new File(this.neo4jwrapp) { Path = change.Path, Commit = c.Sha };
                                this.neo4jwrapp.WriteNode(fileNode);
                                switch (change.Status)
                                {
                                    case ChangeKind.Modified:
                                        this.neo4jwrapp.WriteEdge(new GraphClasses.Node.Commit(null) { Sha = c.Sha }, fileNode, new ModifiedFile(this.neo4jwrapp));
                                        break;
                                    case ChangeKind.Renamed:
                                        this.neo4jwrapp.WriteEdge(new GraphClasses.Node.Commit(null) { Sha = c.Sha }, fileNode, new RenamedFile(this.neo4jwrapp));                                        
                                        break;
                                    case ChangeKind.Added:                                        
                                        this.neo4jwrapp.WriteEdge(new GraphClasses.Node.Commit(null) { Sha = c.Sha }, fileNode, new CreatedFile(this.neo4jwrapp));
                                        break;
                                    default:
                                        int useForBreakPoint = 0;
                                        this.neo4jwrapp.WriteEdge(new GraphClasses.Node.Commit(null) { Sha = c.Sha }, fileNode, new ModifiedFile(this.neo4jwrapp));
                                        break;
                                }
                            }
                            else
                            {
                                switch (change.Status)
                                {
                                    case ChangeKind.Deleted:
                                        {
                                            // Do nothing ... will later create an edge to the deleted file node
                                        }
                                        break;


                                    default:
                                        int useForBreakPoint = 0;
                                        break;

                                }
                            }
                        }
                    }
                }
                else
                {
                    // do initial commit
                    foreach (var element in CommitAnalyzser.GetAllLeafesInTree(c.Tree))
                    {
                        {
                            var fileSearch= new File() { Path = element.Path, Commit = c.Sha };
                            if (neo4jwrapp.Find<BaseNode>(fileSearch).Any())
                            {
                                continue;
                            }
                        }


                        var fileNode = new File(this.neo4jwrapp) { Path = element.Path, Commit = c.Sha };                       

                        this.neo4jwrapp.WriteNode(fileNode);
                        this.neo4jwrapp.WriteEdge(new GraphClasses.Node.Commit(null) { Sha = c.Sha}, fileNode, new CreatedFile(this.neo4jwrapp));                        
                    }
                }                             
            }
        }
    }

    internal class CommitAnalyzser
    {
      

     
        public static IEnumerable<TreeEntry> GetAllLeafesInTree(Tree t)
        {
            foreach (var e in t)
            {
                switch (e.TargetType)
                {
                    case TreeEntryTargetType.Tree:
                        foreach (var r in GetAllLeafesInTree(e.Target as Tree))
                        {
                            yield return r;
                        }
                        break;

                    default:
                        yield return e;
                        break;
                }
            }
        }
    }

}
