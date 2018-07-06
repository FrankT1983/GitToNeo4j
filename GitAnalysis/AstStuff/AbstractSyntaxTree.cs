using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GitAnalysis
{
    [DebuggerDisplay("{Kind} : {SpanStart} - {SpanEnd}")]
    public class AbstractSyntaxNode
    {
        public string Kind { get; set; }
        public int SpanStart { get; set; }
        public int SpanLenght { get; set; }

        public int SpanEnd { get { return this.SpanStart + this.SpanLenght; } }

        public int Id { get; set; }

        public List<AbstractSyntaxNode> Children = new List<AbstractSyntaxNode>();

        public AbstractSyntaxNode(SyntaxNode fromRoslynNode, ref int id)
        {
            this.Kind = fromRoslynNode.Kind().ToString();
            this.SpanStart = fromRoslynNode.SpanStart;
            this.SpanLenght = fromRoslynNode.Span.Length;
            this.Id = id;
            id++;

            foreach (var c in fromRoslynNode.ChildNodes())
            {
                this.Children.Add(new AbstractSyntaxNode(c, ref id));
            }            
        }

        private static string DefaultSeperator = "||";

        internal string ToLineString()
        {
            var l = new List<string>() { this.Id.ToString(), this.Kind, this.SpanStart.ToString(), this.SpanLenght.ToString() };
            return String.Join(DefaultSeperator, l);
        }
    }

    public class AbstractSyntaxTree
    {
        AbstractSyntaxNode root;

        public AbstractSyntaxTree(AbstractSyntaxNode root)
        {
            this.root = root;
        }


        internal static AbstractSyntaxTree FromString(string content)
        {
            if (String.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            var foo = CSharpSyntaxTree.ParseText(content);
            int startId = 0;
            var rootNode = new AbstractSyntaxNode(foo.GetRoot(), ref startId);

            return new AbstractSyntaxTree(rootNode);
        }

        public IEnumerable<AbstractSyntaxNode> PreOrderTraversal()
        {
            return PreOrderTraversal(this.root);
        }

        private IEnumerable<AbstractSyntaxNode> PreOrderTraversal(AbstractSyntaxNode root)
        {
            yield return root;
            foreach (var c in root.Children)
            {
                foreach (var c2 in PreOrderTraversal(c))
                {
                    yield return c2;
                }
            }
        }

        internal void Serialize(string savePath)
        {
            var allChildren = PreOrderTraversal().ToList();
            allChildren.Sort((c1, c2) => c1.Id.CompareTo(c2.Id));

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(savePath))
            {
                foreach (var c in allChildren)
                {
                    file.WriteLine(c.ToLineString());
                }
            }
        }

        internal static AbstractSyntaxTree Deserialize(string folderPath)
        {
            using (System.IO.StreamReader file = new System.IO.StreamReader(folderPath))
            {

            }
            return null;
        }

        internal AbstractSyntaxNode GetRoot()
        {
            return this.root;
        }
    }
}