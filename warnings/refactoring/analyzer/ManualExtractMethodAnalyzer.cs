using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using Roslyn.Compilers.CSharp;
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

            var parentBlock = ASTUtil.getBlockOfMethod(parentMethodDeclarationBefore);
            var extractedMethodBlock = ASTUtil.getBlockOfMethod(extractedMethodDeclaration);
            
            // TODO: to finish



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
