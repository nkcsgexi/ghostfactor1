using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using Roslyn.Compilers.CSharp;
using warnings.util;

namespace warnings.analyzer
{

    /* Analyzer for any syntax node. */
    public interface ISyntaxNodeAnalyzer
    {
        void SetSyntaxNode(SyntaxNode node);

        /* Are the nodes neighbors, means no code in between. */
        bool IsNeighborredWith(SyntaxNode another);

        /* Get the common parent of these two nodes. */
        SyntaxNode GetCommonParent(SyntaxNode another);

        /* Get a tree-like structure depicting all the decendent nodes and itself. */
        string DumpTree();
    }

    internal class SyntaxNodeAnalyzer : ISyntaxNodeAnalyzer
    {
        private static int ANALYZER_COUNT = 0;

        public static int GetCount()
        {
            return ANALYZER_COUNT;
        }

        private SyntaxNode node;

        private readonly Logger logger;

        internal SyntaxNodeAnalyzer()
        {
            Interlocked.Increment(ref ANALYZER_COUNT);
            logger = NLoggerUtil.getNLogger(typeof (SyntaxNodeAnalyzer));
        }

        ~SyntaxNodeAnalyzer()
        {
            Interlocked.Decrement(ref ANALYZER_COUNT);
        }

        public void SetSyntaxNode(SyntaxNode node)
        {
            this.node = node;
        }

        public bool IsNeighborredWith(SyntaxNode another)
        {
            // Get the nearest common acncestor.
            var parent = GetCommonParent(another);

            // If the ancestor has decendent whose span is between node and another node, then they are not neighborored,
            // otherwise they are neighbors.
            return !parent.DescendantNodes().Any(
                // n should between node and another node.
                n => n.Span.CompareTo(node.Span) * n.Span.CompareTo(another.Span) < 0
                // and n should not overlap with node and another node. 
                && !n.Span.OverlapsWith(node.Span)
                && !n.Span.OverlapsWith(another.Span));
        }

        public SyntaxNode GetCommonParent(SyntaxNode another)
        {
            // Get list of common ancestors.
            var commonAncestors = node.Ancestors().Where(a => a.Span.Contains(another.Span));
            
            // Sort the list by span length.
            var sortedCommonAncestors = commonAncestors.OrderBy(n => n.Span.Length);
            
            // The ancestor of the least length is the nearest common ancestor.
            return sortedCommonAncestors.First();
        }

        public string DumpTree()
        {
            return Environment.NewLine + PrintPretty(node, "", true);
        }


        /* algorithm copied from http://stackoverflow.com/questions/1649027/how-do-i-print-out-a-tree-structure . */
        private string PrintPretty(SyntaxNode node,string indent, bool last)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(indent);
            if (last)
            {
                sb.Append("\\-");
                indent += "\t";
            }
            else
            {
                sb.Append("|-");
                indent += "|\t";
            }
            sb.AppendLine(node.Kind.ToString() + ":" + StringUtil.ReplaceNewLine(node.GetText(), ""));

            for (int i = 0; i < node.ChildNodes().Count() ; i++)
            {
                var child = node.ChildNodes().ElementAt(i);
                sb.Append(PrintPretty(child, indent, i == node.ChildNodes().Count() - 1));
            }

            return sb.ToString();
        }
    }

}
