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

        private string RepoToClone = "";
        private string LocalDestiantion = "";
        private AnalysisBase wrapper;


        public ViewModel()
        {
            cloneWorker.DoWork += new DoWorkEventHandler((obj, arg) => CloneGit());
            commitWorker.DoWork += new DoWorkEventHandler((obj, arg) => { Update(arg.Argument as AnalysisOptions); AnalyzeGit(); });                
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
        }

        private void AnalyzeGit()
        {         
            
            try
            {
                wrapper.WriteCommitNodes();
                this.StatusChanged("Finished Writing nodes");
            }
            catch(Exception e)
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