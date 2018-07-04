using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitAnalysis.GraphClasses.Node
{
    public class Person : BaseNode
    {
        public Person(Neo4jWrapper neo4jwrapp) : base(neo4jwrapp)
        {
        }

        public Person() : base(-1)
        {

        }

        public string Name { get; internal set; }
        public string EMail { get; internal set; }
    }

}
