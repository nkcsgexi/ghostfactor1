using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Documents;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using warnings.util;

namespace warnings.analyzer
{
    /* Analyzer for a method declaration. */
    public interface IMethodAnalyzer
    {
        void SetMethodDeclaration(SyntaxNode method);
        SyntaxNode GetMethodName();

        /* Statement related queries. */
        IEnumerable<SyntaxNode> GetStatements();
        IEnumerable<SyntaxNode> GetStatementsByIndexRange(int start, int end);
        IEnumerable<SyntaxNode> GetStatementsBefore(int position);
        SyntaxNode GetStatementAt(int position);
        IEnumerable<SyntaxNode> GetStatementsAfter(int position);
        IEnumerable<SyntaxNode> GetReturnStatements();
        
        /* Parameter related queries. */
        IEnumerable<SyntaxNode> GetParameters();
        IEnumerable<IEnumerable<SyntaxNode>> GetParameterUsages();

        SyntaxNode GetReturnType();
        bool HasReturnStatement();
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
        private Logger logger = NLoggerUtil.getNLogger(typeof(MethodAnalyzer));

        internal MethodAnalyzer()
        {
            Interlocked.Increment(ref ANALYZER_COUNT);
        }

        ~MethodAnalyzer()
        {
            Interlocked.Decrement(ref ANALYZER_COUNT);
        }

        public void SetMethodDeclaration(SyntaxNode method)
        {
            this.method = (MethodDeclarationSyntax) method;   
        }

        public SyntaxNode GetMethodName()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SyntaxNode> GetStatements()
        {
            BlockSyntax block = ASTUtil.GetBlockOfMethod(method);
            var statements = new StatementSyntax[] {};
            if (block != null)
                statements = ASTUtil.getStatementsInBlock(block);
            return statements.OrderBy(n => n.Span.Start).AsEnumerable();
         }

        /* Get a subset of all the containing statements, start and end index are inclusive. */
        public IEnumerable<SyntaxNode> GetStatementsByIndexRange(int start, int end)
        {
            var statements = GetStatements();
            var subList = new List<SyntaxNode>();
            for(int i = start; i <= end; i++)
            {
                subList.Add(statements.ElementAt(i));
            }
            return subList.AsEnumerable();
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

        public IEnumerable<IEnumerable<SyntaxNode>> GetParameterUsages()
        {
            // Containing the results.
            var list = new List<IEnumerable<SyntaxNode>>();
            
            // All the parameters taken.
            var parameters = GetParameters();

            // Block of the method declaration.
            var block = ASTUtil.GetBlockOfMethod(method);

            // For each parameter.
            foreach (ParameterSyntax para in parameters)
            {
                // If an identifier name equals the paraemeter's name, it is one usage of the 
                // parameter.
                var usage = block.DescendantNodes().Where(n => n.Kind == SyntaxKind.IdentifierName 
                    && n.GetText().Equals(para.Identifier.ValueText));
                list.Add(usage);
            }
            return list.AsEnumerable();
        }

        public SyntaxNode GetReturnType()
        {
            // The return type's span start shall before the limit.
            int limit = 0;

            // Get the para list of this method.
            var paras = method.DescendantNodes().First(n => n.Kind == SyntaxKind.ParameterList);
            if (paras == null)
            {
                var block = ASTUtil.GetBlockOfMethod(method);
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

        public IEnumerable<SyntaxNode> GetReturnStatements()
        {
            return method.DescendantNodes().Where(n => n.Kind == SyntaxKind.ReturnStatement);
        }

        public bool HasReturnStatement()
        {
            // Get the return statement of the method.
            var returns = method.DescendantNodes().Where(n => n.Kind == SyntaxKind.ReturnStatement);
            
            // Return if no such statement.
            return returns.Any();

        }

        public string DumpTree()
        {
            var analyzer = AnalyzerFactory.GetSyntaxNodeAnalyzer();
            analyzer.SetSyntaxNode(method);
            return analyzer.DumpTree();
        }
    }
}
