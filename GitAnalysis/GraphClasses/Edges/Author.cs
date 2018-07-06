﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitAnalysis.GraphClasses.Edges
{
    public class Author : BaseEdge
    {
        public Author(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public Author() : base(-1)
        {
        }
    }
}
