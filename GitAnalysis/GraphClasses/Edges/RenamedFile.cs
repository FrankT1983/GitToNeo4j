﻿namespace GitAnalysis.GraphClasses.Edges
{
    public class RenamedFile : BaseEdge
    {
        public string ParrentCommit { get; internal set; }
        public string ChangedNameFrom { get; internal set; }

        public RenamedFile(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public RenamedFile() : base(-1)
        {
        }
    }

}
