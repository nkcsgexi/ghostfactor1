using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using warnings.util;

namespace warnings.analyzer
{
    public interface IStatementAnalyzer
    {
        void SetSource(string source);
        void SetSyntaxNode(SyntaxNode statement);
        bool IsStatement();
        bool HasMethodInvocation(string methodName);
    }

    internal class StatementAnalyzer : IStatementAnalyzer
    {
        private string source;
        private SyntaxNode statement;

        public void SetSource(string source)
        {
            this.source = source;
            statement = ASTUtil.getSyntaxTreeFromSource(this.source).GetRoot();
        }

        public void SetSyntaxNode(SyntaxNode statement)
        {
            this.statement = statement;
            this.source = statement.GetText();
        }

        public bool IsStatement()
        {
            return statement is StatementSyntax;
        }

        public bool HasMethodInvocation(string methodName)
        {
            IEnumerable<SyntaxNode> nodes = statement.DescendantNodes();
            foreach (SyntaxNode n in nodes)
            {
                // select the node if it is invocation of a method
                if (n is InvocationExpressionSyntax)
                {
                    // first node in the invocation should be the method name, including member access
                    // expression.
                    String method = n.DescendantNodes().First().GetText();
                    if (method.EndsWith(methodName))
                        return true;
                }
            }
            return false;
        }
    }
}
