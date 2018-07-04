using System;
using System.Collections.Generic;
using System.Text;

namespace GitAnalyzsisTools
{
    public class AnalysisBase
    {
        public string RepoCloneFolder { get; }
        public string AnalysisDestination { get; }
        public Repository Repo { get; }


        public AnalysisBase(string repoCloneFolder, string analysisDestinatinFolder)
        {
            this.RepoCloneFolder = repoCloneFolder;
            this.AnalysisDestination = analysisDestinatinFolder + "\\";
            this.Repo = new Repository(this.RepoCloneFolder);
        }
    }
}
