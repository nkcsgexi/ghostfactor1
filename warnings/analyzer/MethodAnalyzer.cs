using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Documents;
using Roslyn.Compilers.CSharp;
using warnings.util;

namespace warnings.analyzer
{
    /* Analyzer for a method declaration. */
    public interface IMethodAnalyzer
    {
        void SetMethodDeclaration(MethodDeclarationSyntax method);
        IEnumerable<SyntaxNode> GetStatements();
        IEnumerable<SyntaxNode> GetStatementsBefore(int position);
        SyntaxNode GetStatementAt(int position);
        IEnumerable<SyntaxNode> GetStatementsAfter(int position); 
        IEnumerable<SyntaxNode> GetParameters();
        SyntaxNode GetReturnType();
        string DumpTree();
    }

    internal class MethodAnalyzer : IMethodAnalyzer
    {
        private static int ANALYZER_COUNT = 0;

        public static int GetCount()
        {
            return ANALYZER_COUNT;
        }

        private MethodDeclarationSyntax method;

        internal MethodAnalyzer()
        {
            Interlocked.Increment(ref ANALYZER_COUNT);
        }

        ~MethodAnalyzer()
        {
            Interlocked.Decrement(ref ANALYZER_COUNT);
        }

        public void SetMethodDeclaration(MethodDeclarationSyntax method)
        {
            this.method = method;   
        }

        public IEnumerable<SyntaxNode> GetStatements()
        {
            BlockSyntax block = ASTUtil.getBlockOfMethod(method);
            StatementSyntax[] statements = new StatementSyntax[] {};
            if (block != null)
                statements = ASTUtil.getStatementsInBlock(block);
            return statements.OrderBy(n => n.Span.Start).AsEnumerable();
         }

        public IEnumerable<SyntaxNode> GetStatementsBefore(int position)
        {
            // Get all the statements first.
            IEnumerable<SyntaxNode> statements = GetStatements();

            // Initiate an empty statement list.
            IList<SyntaxNode> result = new List<SyntaxNode>(); 
            
            // Iterate all the statement.
            foreach(var statement in statements)
            {
                // For statement whose end point is before the position, add it to the result
                if (statement.Span.End < position)
                    result.Add(statement);
            }
            return result.AsEnumerable();
        }

        public SyntaxNode GetStatementAt(int position)
        {
            // Get all the statements first.
            IEnumerable<SyntaxNode> statements = GetStatements();
            
            // Select the first statement whose span intersects with the position.
            return statements.First(s => s.Span.IntersectsWith(position));
        }

        public IEnumerable<SyntaxNode> GetStatementsAfter(int position)
        {
            // Get all the statements first.
            IEnumerable<SyntaxNode> statements = GetStatements();

            // Initiate an empty statement list.
            IList<SyntaxNode> result = new List<SyntaxNode>();

            // Iterate all the statement.
            foreach (var statement in statements)
            {
                // For statement whose end point is after the position, add it to the result
                if (statement.Span.Start > position)
                    result.Add(statement);
            }
            return result.AsEnumerable();
        }

        public IEnumerable<SyntaxNode> GetParameters()
        {
            // Any node that in the parameter type, different from argument type
            return method.DescendantNodes().Where(n => n.Kind == SyntaxKind.Parameter);
        }

        public SyntaxNode GetReturnType()
        {
            // The return type's span start shall before the limit.
            int limit = 0;

            // Get the para list of this method.
            var paras = method.DescendantNodes().First(n => n.Kind == SyntaxKind.ParameterList);
            if (paras == null)
            {
                var block = ASTUtil.getBlockOfMethod(method);
                limit = block.Span.Start;
            }
            else
            {
                limit = paras.Span.Start;
            }
            // Get all the predefined types before the limit, such as void, int and long.
            var types = method.DescendantNodes().Where(n => n.Kind == SyntaxKind.PredefinedType &&
                                                                    n.Span.Start < limit);

            // Get all the generic types from the limt, such as IEnumerable<int>.
            var genericTypes = method.DescendantNodes().Where(n => n.Kind == SyntaxKind.GenericName &&
                                                                   n.Span.Start < limit);
            
            
            // Get all identifiers before the limit.
            var identifiers = method.DescendantNodes().Where(n => n.Kind == SyntaxKind.IdentifierName &&
                                                                  n.Span.Start < limit);

            if (types.Count() == 1)
                return types.First();

            if (genericTypes.Count() == 1)
                return genericTypes.First();
            
            int leftMost = int.MaxValue;
            SyntaxNode leftMostIdentifier = null;

            // For all the identifiers, the leftmost one should be the return type.
            foreach (SyntaxNode node in identifiers)
            {
                if (node.Span.Start < leftMost)
                {
                    leftMost = node.Span.Start;
                    leftMostIdentifier = node;
                }
            }

            return leftMostIdentifier;
        }

        public string DumpTree()
        {
            var analyzer = AnalyzerFactory.GetSyntaxNodeAnalyzer();
            analyzer.SetSyntaxNode(method);
            return analyzer.DumpTree();
        }
    }
}
