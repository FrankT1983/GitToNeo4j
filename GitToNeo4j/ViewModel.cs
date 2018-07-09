using System;
using System.ComponentModel;
using GitAnalysis;
using LibGit2Sharp;

namespace GitToNeo4j
{
    internal class ViewModel
    {
        private BackgroundWorker cloneWorker = new BackgroundWorker();
        private BackgroundWorker commitWorker = new BackgroundWorker();
        private BackgroundWorker astWorker = new BackgroundWorker();
        private BackgroundWorker astLinkWorker = new BackgroundWorker();

        private string RepoToClone = "";
        private string LocalDestiantion = "";
        private AnalysisBase wrapper;


        public ViewModel()
        {
            cloneWorker.DoWork += new DoWorkEventHandler((obj, arg) => CloneGit());
            commitWorker.DoWork += new DoWorkEventHandler((obj, arg) => { Update(arg.Argument as AnalysisOptions); AnalyzeGit(); });
            astWorker.DoWork += new DoWorkEventHandler((obj, arg) => { Update(arg.Argument as AnalysisOptions); AnalyzeAst(); });
            astLinkWorker.DoWork += new DoWorkEventHandler((obj, arg) => { Update(arg.Argument as AnalysisOptions); LinkAsts(); });
        }        

        public delegate void StatusChangedHandler(string UpdateText);
        public delegate void ProgressChangedHandler(double percentage);

        public event StatusChangedHandler StatusChanged;
        public event ProgressChangedHandler ProgressChanged;

        internal void StartCloning(string repo, string targetDirt)
        {
            if (this.cloneWorker.IsBusy) { return; }
            this.RepoToClone = repo;
            this.LocalDestiantion = targetDirt;

            this.cloneWorker.RunWorkerAsync();
        }
                

        private void CloneGit()
        {
            string clone = this.RepoToClone;
            string lokalPath = this.LocalDestiantion;
            if (String.IsNullOrWhiteSpace(clone))
            { return; }

            if (String.IsNullOrWhiteSpace(lokalPath))
            { return; }

            FireStatusUpdate("Start Cloning");
            try
            {
                Repository.Clone(clone, lokalPath);
            }
            catch (Exception e)
            {
                FireStatusUpdate("Error cloning: " + e.Message);
                return;
            }
            FireStatusUpdate("Clone Finished");
        }               

        internal void ClearDb()
        {            
            this.wrapper.Clear();
            this.StatusChanged("Cleared DB");
        }

        internal void CommitsToDb(AnalysisOptions options)
        {
            if (this.commitWorker.IsBusy) { return; }
            this.commitWorker.RunWorkerAsync(options);
        }

        internal void ParseAst(AnalysisOptions options)
        {
            if (this.astWorker.IsBusy) { return; }
            this.astWorker.RunWorkerAsync(options);
        }

        internal void LinkAsts(AnalysisOptions options)
        {
            if (this.astLinkWorker.IsBusy) { return; }
            this.astLinkWorker.RunWorkerAsync(options);
        }

        private void FireStatusUpdate(string update)
        {
            if (this.StatusChanged == null) return;
            this.StatusChanged(update);
        }
        

        private void FireProgressUpdate(double percentage)
        {
            if (this.ProgressChanged == null) return;
            this.ProgressChanged(percentage);
        }        

        public void Update(AnalysisOptions option)
        {
            this.wrapper = new AnalysisBase(option.LocalPath, option.AnalysisPath);
            this.wrapper.ProgressChanged += (per) => this.FireProgressUpdate(per);
            this.wrapper.StatusChanged += (stat) => this.FireStatusUpdate(stat);
        }

        private void AnalyzeGit()
        {                    
            try
            {                
                wrapper.WriteCommitNodes();
                this.ProgressChanged(-1);
                this.StatusChanged("Finished Writing nodes");
            }
            catch(Exception e)
            {
                this.StatusChanged(e.Message);
            }
        }


        private void AnalyzeAst()
        {
            try
            {
                this.StatusChanged("Writing Abstract Syntax tree");
                wrapper.WriteAst();
                this.StatusChanged("Finished Writing Abstract Syntax tree");
            }
            catch (Exception e)
            {
                this.StatusChanged(e.Message);
            }
        }

        private void LinkAsts()
        {
            try
            {
                this.StatusChanged("Start Linking Abstract Syntax Trees");
                wrapper.LinkAst();
                this.StatusChanged("Finished Linking Abstract Syntax Trees");
            }
            catch (Exception e)
            {
                this.StatusChanged(e.Message);
            }
        }

        internal class AnalysisOptions
        {
            public AnalysisOptions()
            {
            }

            public string LocalPath { get; set; }
            public string AnalysisPath { get; set; }
        }
    }
}