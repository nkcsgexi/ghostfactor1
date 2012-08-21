using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;

namespace warnings.util
{
    public class StatementAnalyzer
    {
        private string statement;
        private SyntaxNode root;

        public StatementAnalyzer(string source)
        {
            statement = source;
            root = ASTUtil.getSyntaxTreeFromSource(statement).Root;
        }

        public bool isStatement()
        {
            return root is StatementSyntax;
        }

        public Boolean hasMethodInvocation(String methodName)
        {
            IEnumerable<SyntaxNode> nodes = root.DescendentNodes();
            foreach (SyntaxNode n in nodes)
            {
                // select the node if it is invocation of a method
                if (n is InvocationExpressionSyntax)
                {
                    // first node in the invocation should be the method name, including member access
                    // expression.
                    String method = n.DescendentNodes().First().GetText();
                    if (method.EndsWith(methodName))
                        return true;
                }
            }
            return false;
        }
    }
}
