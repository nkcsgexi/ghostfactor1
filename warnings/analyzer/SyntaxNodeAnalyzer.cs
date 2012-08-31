using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Roslyn.Compilers.CSharp;
using warnings.util;

namespace warnings.analyzer
{

    /* Analyzer for any syntax node. */
    public interface ISyntaxNodeAnalyzer
    {
        void SetSyntaxNode(SyntaxNode node);

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

        internal SyntaxNodeAnalyzer()
        {
            Interlocked.Increment(ref ANALYZER_COUNT);
        }

        ~SyntaxNodeAnalyzer()
        {
            Interlocked.Decrement(ref ANALYZER_COUNT);
        }

        public void SetSyntaxNode(SyntaxNode node)
        {
            this.node = node;
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
            sb.AppendLine(node.Kind.ToString() + ":" + StringUtil.replaceNewLine(node.GetText(), ""));

            for (int i = 0; i < node.ChildNodes().Count() ; i++)
            {
                var child = node.ChildNodes().ElementAt(i);
                sb.Append(PrintPretty(child, indent, i == node.ChildNodes().Count() - 1));
            }

            return sb.ToString();
        }
    }
}
