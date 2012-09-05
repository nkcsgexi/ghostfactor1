using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using Roslyn.Compilers.CSharp;
using warnings.analyzer;
using warnings.util;

namespace warnings.refactoring
{
    /* 
     * Analyzer for manual extract method refactoring, generating IManualRefactoring intance by refactoring affected
     * nodes.
     */
    public interface IManualExtractMethodAnalyzer
    {
        /* Set the method declaration of the original method before extracting some part.*/
        void SetMethodDeclarationBeforeExtracting(SyntaxNode declaration);

        /* Set declaration node of the newly created method. */
        void SetExtractedMethodDeclaration(SyntaxNode declaration);

        /* Set the invocation node of the extracted method. */
        void SetInvocationExpression(SyntaxNode invocation);

        /* Get the method declaration after extracting some part. */
        SyntaxNode GetInvokingMethod();

        /* Create instance of IManualRefactoring by the given informations. */
        IManualExtractMethodRefactoring GetRefactoring();
    }

    internal class ManualExtractMethodAnalyzer : IManualExtractMethodAnalyzer
    {
        private SyntaxNode parentMethodDeclarationBefore;

        private SyntaxNode parentMethodDeclarationAfter;

        private SyntaxNode extractedMethodDeclaration;

        private SyntaxNode invocation;


        public void SetMethodDeclarationBeforeExtracting(SyntaxNode parentMethodDeclarationBefore)
        {
            this.parentMethodDeclarationBefore = parentMethodDeclarationBefore;
        }

        public void SetExtractedMethodDeclaration(SyntaxNode extractedMethodDeclaration)
        {
            this.extractedMethodDeclaration = extractedMethodDeclaration;
        }

        public void SetInvocationExpression(SyntaxNode invocation)
        {
            this.invocation = invocation;
            parentMethodDeclarationAfter = GetInvokingMethod();
        }

        public SyntaxNode GetInvokingMethod()
        {
            return invocation.Ancestors().First(n => n.Kind == SyntaxKind.MethodDeclaration);
        }

        public IManualExtractMethodRefactoring GetRefactoring()
        {
            var commonStatements = new List<SyntaxNode>();
            var commonExpressions = new List<SyntaxNode>();

            // Get the block of the original method before extracting anything.
            var parentBlockBefore = ASTUtil.GetBlockOfMethod(parentMethodDeclarationBefore);

            // Get the block of the extracted method declaration.
            var extractedMethodBlock = ASTUtil.GetBlockOfMethod(extractedMethodDeclaration);
            
            // For each decendent node in the block of original method.
            foreach(var node in parentBlockBefore.DescendantNodes())
            {
                // Care about statements.
                if(node is StatementSyntax)
                {
                    // For each statement in the body of the extracted method
                    foreach (var statement in extractedMethodBlock.DescendantNodes().Where(n => n is StatementSyntax))
                    {
                        // If we some how think they are same, memorized that.
                        if(AreStatementsEqual(statement, node))
                        {
                            commonStatements.Add(node);
                        }
                    }
                }

                // Care about expressions.
                if(node is ExpressionSyntax)
                {
                    // For each expression in the block of the extracted method.
                    foreach (var expression in extractedMethodBlock.DescendantNodes().Where(n => n is ExpressionSyntax))
                    {
                        // If we somehow think they are same, memorize that.
                        if(AreExpressionsEqual(expression, node))
                        {
                            commonExpressions.Add(node);
                        }
                    }
                }
            }

            var analyzer = AnalyzerFactory.GetSyntaxNodesAnalyzer();

            // First-class customer: has statements in common.
            if(commonStatements.Any())
            {
                analyzer.SetSyntaxNodes(commonStatements);

                // Get the longest group of statements that are neighbors.
                var extractedStatements = analyzer.GetLongestNeighborredNodesGroup();
                return ManualRefactoringFactory.CreateManualExtractMethodRefactoring
                    (extractedMethodDeclaration, invocation, extractedStatements);
            }  
            
            // Second class customer: has expressions in common.
            if (commonExpressions.Any())
            {
                analyzer.SetSyntaxNodes(commonExpressions);

                // Get the longest node among all the expressions; It is not possible to extract
                // several expressions at the same time. 
                var extractedExpression = analyzer.GetLongestNode();
                return ManualRefactoringFactory.CreateManualExtractMethodRefactoring
                    (extractedMethodDeclaration, invocation, extractedExpression);
            }
            return null;
        }

        private bool AreStatementsEqual(SyntaxNode one, SyntaxNode two)
        {
            return one.GetText().Replace(" ", "").Equals(two.GetText().Replace(" ", ""));
        }

        private bool AreExpressionsEqual(SyntaxNode one, SyntaxNode two)
        {
            return one.GetText().Replace(" ", "").Equals(two.GetText().Replace(" ", ""));
        }
    }
}
