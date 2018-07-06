using GitAnalysis.GraphClasses.Node;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;

namespace GitAnalysis
{
    public static class CypherFluentExtension
    {     
        public static ICypherFluentQuery MatchQuerry(this ICypherFluentQuery cypher, string varialbeName, BaseNode node)
        {
            if (node is Person)
            {               
              return cypher.MatchQuerry(varialbeName, (Person)node);
            }
            if (node is Commit)
            {
                return cypher.MatchQuerry(varialbeName, (Commit)node);
            }
            if (node is File)
            {
                return cypher.MatchQuerry(varialbeName, (File)node);
            }
            if (node is AstElement)
            {
                return cypher.MatchQuerry(varialbeName, (AstElement)node);
            }

            string matchClause = "(" + varialbeName + ":" + node.GetType().Name + ")";
            if (node.Id > 0)
            {
                return cypher.Match(matchClause).Where(varialbeName + ".Id=" + node.Id);
            }

            throw new NotImplementedException("Needs a specialices MatchQuery for " + node.GetType().Name);
        }
        
        public static ICypherFluentQuery MatchQuerry(this ICypherFluentQuery cypher, string varialbeName, Person node)
        {
            string matchClause = "(" + varialbeName + ":" + node.GetType().Name + ")";           
            string whereClause = varialbeName + "." + nameof(node.Name) + "=\"" + node.Name +"\""+
                                        " and  " + 
                                     varialbeName + "." + nameof(node.EMail) + "=\"" + node.EMail + "\"";
            return cypher.Match(matchClause).Where(whereClause);           
        }

        public static ICypherFluentQuery MatchQuerry(this ICypherFluentQuery cypher, string varialbeName, Commit node)
        {
            string matchClause = "(" + varialbeName + ":" + node.GetType().Name + ")";
            string whereClause = varialbeName + "." + nameof(node.Sha) + "=\"" + node.Sha + "\"";                                        
            return cypher.Match(matchClause).Where(whereClause);          
        }

        public static ICypherFluentQuery MatchQuerry(this ICypherFluentQuery cypher, string varialbeName, File node)
        {
            string matchClause = "(" + varialbeName + ":" + node.GetType().Name + ")";
            string whereClause = varialbeName + "." + nameof(node.Commit) + "=\"" + node.Commit + "\"" +
                                    " and  " +
                                varialbeName + "." + nameof(node.Path) + "=\"" + node.Path + "\""; 
            return cypher.Match(matchClause).Where(whereClause);
        }

        public static ICypherFluentQuery MatchQuerry(this ICypherFluentQuery cypher, string varialbeName, AstElement node)
        {
            string matchClause = "(" + varialbeName + ":" + node.GetType().Name + ")";
            
            var wherClause = new List<String>();

            if (node.AstId > -1)
            {
                wherClause.Add(varialbeName + "." + nameof(node.AstId) + "=" + node.AstId);
            }

            if (!String.IsNullOrWhiteSpace(node.FilePath))
            {
                wherClause.Add(varialbeName + "." + nameof(node.FilePath) + "=\"" + node.FilePath + "\"");
            }

            if (!String.IsNullOrWhiteSpace(node.CommitSha))
            {
                wherClause.Add(varialbeName + "." + nameof(node.CommitSha) + "=\"" + node.CommitSha + "\"");
            }
            return cypher.Match(matchClause).Where(String.Join(" and " , wherClause));
        }
    }
}