using GitAnalysis.AstStuff;
using GitAnalysis.AstStuff.InternalGraph;
using GitAnalysis.GraphClasses.Edges;
using GitAnalysis.GraphClasses.Node;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;


namespace GitAnalysis
{
    public class AnalysisBase
    {
        public delegate void StatusChangedHandler(string UpdateText);
        public delegate void ProgressChangedHandler(double percentage);

        public event StatusChangedHandler StatusChanged;
        public event ProgressChangedHandler ProgressChanged;

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


            this.FireStatusChanged("Add Persons.");
            AddPersonsToGraph(commits);
            this.FireStatusChanged("Finished Persons, write Commits and link to persons");
            AddCommitsAndLinkToPersons(commits);
            this.FireStatusChanged("Finished Person <-> Commit, Write Commit <-> Commit");
            AddCommitParentChildDependencies(commits);
            this.FireStatusChanged("Finished Commit <-> Commit, Write Commit <-> File");
            AddCommitFiles(commits);

            this.FireStatusChanged("Finished File <-> Commit, Write File <-> File");
            AddFilesToFile(commits);
            this.FireStatusChanged("Finished File <-> File");

            this.neo4jwrapp.UpdateInfo();
        }

        private void FireStatusChanged(string update)
        {
            if (this.StatusChanged == null) return;
            this.StatusChanged(update);
        }

        private void FireProgressChanged(double percentage)
        {
            if (this.ProgressChanged == null) return;
            this.ProgressChanged(percentage);
        }

        private List<LibGit2Sharp.Commit> CommitsByTime()
        {
            var commits = new List<LibGit2Sharp.Commit>(Repo.Commits);
            commits.Sort((a, b) => a.Author.When.CompareTo(b.Author.When));
            return commits;
        }

        private void AddPersonsToGraph(List<LibGit2Sharp.Commit> commits)
        {
            var authors = commits.Select(c => c.Author).ToList();

            int i = 0;
            foreach (var a in authors)
            {
                this.FireProgressChanged(i++ / (double)authors.Count);
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
            int i = 0;
            foreach (var c in commits)
            {
                this.FireProgressChanged(i++ / (double)commits.Count);
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
            int i = 0;
            foreach (var c in commits)
            {
                this.FireProgressChanged(i++ / (double)commits.Count);
                foreach (var p in c.Parents)
                {
                    this.neo4jwrapp.WriteEdge(new GraphClasses.Node.Commit(null) { Sha = p.Sha },
                                              new GraphClasses.Node.Commit(null) { Sha = c.Sha },
                                              new NextCommit(this.neo4jwrapp));
                }
            }
        }

        public void WriteAst()
        {
            int i = 0;
            var files = this.neo4jwrapp.FindWithoutLabel<File>(typeof(File), File.HasAstLabel).ToList();
            foreach (var file in files)
            {
                this.FireProgressChanged(i++ / (double)files.Count);
                var fileContent = GetRaw(file.Commit, file.Path);
                var parsedGraph = GumTreeWrapper.Parse(fileContent);

                bool complete = false;
                int triedCount = 0;
                while (!complete && triedCount <3)
                { 
                    try
                    {
                        var foo = parsedGraph.GetRoot();
                        complete = true;
                        
                    } catch (Exception e)
                    {
                        triedCount++;
                        FireStatusChanged("Could not parse with GumTree ... Try again. " + file.Path + " "  + file.Commit);
                    }
                }
                
                if (!complete)
                {
                    FireStatusChanged("Could not parse with GumTree ... Give up ... next File. " + file.Path + " " + file.Commit);
                    continue;
                }

                AddAstGraphToGraph(parsedGraph, file);

                this.neo4jwrapp.AddLabel(file, File.HasAstLabel);
            }
            this.FireProgressChanged(-1);
        }

        public void LinkAst()
        {
            int i = 0;            
            var nextVersionEdges = this.neo4jwrapp.FindAllEdgesBetweenTypes<File, NextFileVersion, File>();
            double count = nextVersionEdges.Count();
            foreach (var e in nextVersionEdges)
            {
                this.FireProgressChanged(i++/count);

                if (this.neo4jwrapp.FindAllEdgesBetweenNodes<File, WithAstTransition, File>(e.From, e.To).Any())
                {
                    continue;
                }


                var fromContent = GetRaw(e.From.Commit, e.From.Path);
                var toContent = GetRaw(e.To.Commit, e.To.Path);

                var transitions = GumTreeWrapper.Compare2(fromContent,toContent);
                
                int triedCount = 0;
                while (transitions == null  && triedCount < 3)
                {
                    try
                    {
                        var foo = GumTreeWrapper.Compare2(fromContent, toContent);                    

                    }
                    catch (Exception )
                    {
                        triedCount++;
                        FireStatusChanged("Could not compare with GumTree ... Try again. ");
                    }
                }


                foreach (var transitionEdge in transitions)
                {
                    if ( String.Compare(transitionEdge.Mode,"Keep") == 0 )
                    {
                        this.neo4jwrapp.AddTransitionEdge(e.From.Commit, e.From.Path, transitionEdge.From, 
                                                          e.To.Commit, e.To.Path, transitionEdge.To, 
                                                          new NoCodeModification(this.neo4jwrapp));
                        continue;
                    }

                    if (String.Compare(transitionEdge.Mode, "Insert") == 0)
                    {
                        this.neo4jwrapp.AddLabel(new AstElement() {CommitSha = e.From.Commit, FilePath = e.From.Path, AstId = transitionEdge.From }, BaseNode.ContainsFile);
                        continue;
                    }

                    if (String.Compare(transitionEdge.Mode, "Modified") == 0)
                    {
                        this.neo4jwrapp.AddTransitionEdge(e.From.Commit, e.From.Path, transitionEdge.From,
                                                          e.To.Commit, e.To.Path, transitionEdge.To,
                                                          new ModifiedCode(this.neo4jwrapp));
                        continue;
                    }

                    if (String.Compare(transitionEdge.Mode, "Delete") == 0)
                    {
                        this.neo4jwrapp.AddTransitionEdge(e.From.Commit, e.From.Path, transitionEdge.From,
                                                          new AstElementDeleted() { CommitSha = e.To.Commit, FilePath = e.To.Path },
                                                          new CodeRemoved(this.neo4jwrapp));
                        continue;
                    }

                    throw new NotImplementedException("Missing Ast Transition for "+transitionEdge.Mode);
                }

                this.neo4jwrapp.WriteEdge(e.From, e.To, new WithAstTransition(this.neo4jwrapp));                
            }

            this.neo4jwrapp.PropagadeModifictionAttribute();
            this.FireProgressChanged(-1);
        }

        private void AddAstGraphToGraph(AstGraph parsedGraph, File rootFile)
        {
            foreach (var n in parsedGraph.Nodes)
            {
                n.Id = this.neo4jwrapp.NextNodeId();
                n.FilePath = rootFile.Path;
                n.CommitSha = rootFile.Commit;
                this.neo4jwrapp.WriteNode(n);
            }

            BaseNode root = parsedGraph.GetRoot();
            if (root != null)
            {
                this.neo4jwrapp.WriteEdge(rootFile, root, new AstOfFile(this.neo4jwrapp));

                var deleteNode = new AstElementDeleted(this.neo4jwrapp) { CommitSha = rootFile.Commit, FilePath = rootFile.Path };
                this.neo4jwrapp.WriteNode(deleteNode);
                this.neo4jwrapp.WriteEdge(rootFile, deleteNode, new AstSpecialNode(this.neo4jwrapp));
            }

            foreach (var e in parsedGraph.Edges)
            {
                var from = this.neo4jwrapp.Find(new AstElement() { AstId = e.From, FilePath = rootFile.Path, CommitSha = rootFile.Commit }).FirstOrDefault();
                var to = this.neo4jwrapp.Find(new AstElement() { AstId = e.To, FilePath = rootFile.Path, CommitSha = rootFile.Commit }).FirstOrDefault();

                if ((from != null) && (to != null))
                {
                    this.neo4jwrapp.WriteEdge(from, to, new AstAbove(this.neo4jwrapp));
                }
            }

        }

        private string GetRaw(string commitSha, string path)
        {

            var commit = Repo.Commits.FirstOrDefault(c => c.Sha == commitSha);
            return CommitAnalyzser.StreamToString(CommitAnalyzser.ContentStreamFromPath(commit, path));
        }

        private void AddFilesToFile(List<LibGit2Sharp.Commit> commits)
        {
            // File <[ContainsFile]-Commit-[nextCommit]->Commit-[ContainsFile]->File mit pfad == 

            // todo: do renames first??
            this.neo4jwrapp.LinkFileBetweenCommits();

            //var edges = this.neo4jwrapp.FindAllEdgesBetweenTypes<GraphClasses.Node.Commit, ModifiedFile, File>();
            //foreach (var e in edges)
            //{
            //    var changeOriginCommit = e.Edge.ChangeParrentCommit;
            //    var currentFilePath = e.To.Path;

            //    var originFile = this.neo4jwrapp.Find<File>(new File() { Path = currentFilePath, Commit = changeOriginCommit }).FirstOrDefault();

            //    if (originFile!=null)
            //    {
            //        this.neo4jwrapp.WriteEdge(originFile, e.To, new NextFileVersion(this.neo4jwrapp));
            //    }
            //}

            this.neo4jwrapp.RemoveIntermediateNoChanges();
        }

        private void AddCommitFiles(List<LibGit2Sharp.Commit> commits)
        {

            int i = 0;
            double count = commits.Count();
            foreach (var c in commits)
            {
                this.FireProgressChanged(i++ / count);
                i++;
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
                                    if (neo4jwrapp.Find<File>(fileSearch).Any())
                                    {
                                        continue;
                                    }
                                }


                                var fileNode = new File(this.neo4jwrapp) { Path = change.Path, Commit = c.Sha };
                                this.neo4jwrapp.WriteNode(fileNode);
                                switch (change.Status)
                                {
                                    case ChangeKind.Modified:
                                        this.neo4jwrapp.WriteEdge(new GraphClasses.Node.Commit(null) { Sha = c.Sha }, fileNode, new ModifiedFile(this.neo4jwrapp) { ChangeParrentCommit= p.Sha }, new ContainsFile(this.neo4jwrapp));
                                        break;
                                    case ChangeKind.Renamed:
                                        this.neo4jwrapp.WriteEdge(new GraphClasses.Node.Commit(null) { Sha = c.Sha }, fileNode, new RenamedFile(this.neo4jwrapp), new ContainsFile(this.neo4jwrapp));
                                        break;
                                    case ChangeKind.Added:
                                        this.neo4jwrapp.WriteEdge(new GraphClasses.Node.Commit(null) { Sha = c.Sha }, fileNode, new CreatedFile(this.neo4jwrapp), new ContainsFile(this.neo4jwrapp));
                                        break;
                                    default:
                                        int useForBreakPoint = 0;
                                        this.neo4jwrapp.WriteEdge(new GraphClasses.Node.Commit(null) { Sha = c.Sha }, fileNode, new ModifiedFile(this.neo4jwrapp) { ChangeParrentCommit = p.Sha }, new ContainsFile(this.neo4jwrapp));
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

                    // add no modification nodes to create complete change history
                    foreach (var element in CommitAnalyzser.GetAllLeafesInTree(c.Tree))
                    {
                        {
                            var fileSearch = new File() { Path = element.Path, Commit = c.Sha };
                            if (neo4jwrapp.Find<BaseNode>(fileSearch).Any())
                            {
                                continue;
                            }
                        }
                        var fileNode = new File(this.neo4jwrapp) { Path = element.Path, Commit = c.Sha };

                        this.neo4jwrapp.WriteNode(fileNode);
                        this.neo4jwrapp.WriteEdge(new GraphClasses.Node.Commit(null) { Sha = c.Sha }, fileNode, new NoModification(this.neo4jwrapp), new ContainsFile(this.neo4jwrapp));
                    }
                }
                else
                {
                    // do initial commit
                    foreach (var element in CommitAnalyzser.GetAllLeafesInTree(c.Tree))
                    {
                        {
                            var fileSearch = new File() { Path = element.Path, Commit = c.Sha };
                            if (neo4jwrapp.Find<BaseNode>(fileSearch).Any())
                            {
                                continue;
                            }
                        }
                        var fileNode = new File(this.neo4jwrapp) { Path = element.Path, Commit = c.Sha };

                        this.neo4jwrapp.WriteNode(fileNode);
                        this.neo4jwrapp.WriteEdge(new GraphClasses.Node.Commit(null) { Sha = c.Sha }, fileNode, new CreatedFile(this.neo4jwrapp), new ContainsFile(this.neo4jwrapp));
                    }
                }
            }
        }
    }
}
