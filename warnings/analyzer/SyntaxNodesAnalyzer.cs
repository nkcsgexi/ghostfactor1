using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace warnings.analyzer
{
    /* Analyzer for a bunch of syntax nodes together. */
    public interface ISyntaxNodesAnalyzer
    {
        void SetSyntaxNodes(IEnumerable<SyntaxNode> nodes);
        int GetStartPosition();
        int GetEndPosition();
    }
    internal class SyntaxNodesAnalyzer : ISyntaxNodesAnalyzer
    {
        private IEnumerable<SyntaxNode> nodes;

        public void SetSyntaxNodes(IEnumerable<SyntaxNode> nodes)
        {
            this.nodes = nodes;
        }

        /* The leftmost postion for all the nodes. */
        public int GetStartPosition()
        {
            int start = int.MaxValue;
            foreach (var syntaxNode in nodes)
            {
                if (syntaxNode.Span.Start < start)
                    start = syntaxNode.Span.Start;
            }
            return start;
        }

        /* The rightmost position for all the nodes. */
        public int GetEndPosition()
        {
            int end = int.MinValue;
            foreach (var node in nodes)
            {
                if (node.Span.End > end)
                    end = node.Span.End;
            }
            return end;
        }
    }
}
