using GitAnalysis.GraphClasses.Node;
using LibGit2Sharp;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GitAnalysis
{
    internal class CommitAnalyzser
    {

        public static string StreamToString(Stream s)
        {
            if (s == null) { return null; }
            using (var tr = new StreamReader(s, Encoding.UTF8))
            {
                return tr.ReadToEnd();
            }
        }

        public static Stream ContentStreamFromPath(LibGit2Sharp.Commit commit, string path)
        {

            var treeItem = commit.Tree[path];
            if (treeItem == null) { return null; }
            var blob = commit.Tree[path].Target as Blob;
            if (blob == null)
            { return null; }

            return blob.GetContentStream();
        }



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
