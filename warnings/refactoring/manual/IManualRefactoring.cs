using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using warnings.analyzer;
using warnings.conditions;
using warnings.util;

namespace warnings.refactoring
{
    /* Refactoring input that shall be feed in to the checker. */
    public interface IManualRefactoring : IHasRefactoringType
    {
        string ToString();
        void MapToDocuments(IDocument before, IDocument after);
    }

    /* public interface for communicateing a manual extract method refactoring.*/
    public interface IManualExtractMethodRefactoring : IManualRefactoring
    {
        /* Method declaration node of the extracted method. */
        SyntaxNode ExtractedMethodDeclaration { get; }

        /* Method invocation node where the extracted method is invoked. */
        SyntaxNode ExtractMethodInvocation { get; }

        /* Statements to extract in the original code. */
        IEnumerable<SyntaxNode> ExtractedStatements { get; }

        /* Expression to extract in the original code. */
        SyntaxNode ExtractedExpression { get; }

    }

    /* public interface for communicating a manual rename refactoring. */
    public interface IManualRenameRefactoring : IManualRefactoring
    {
        
    }


    /* Containing all the information about the extract method information. */
    internal class ManualExtractMethodRefactoring : IManualExtractMethodRefactoring
    {
        /* Method declaration node of the extracted method. */
        public SyntaxNode ExtractedMethodDeclaration { private set; get; }

        /* Method invocation node where the extracted method is invoked. */
        public SyntaxNode ExtractMethodInvocation { private set; get; }

        /* Statements to extract in the original code. */
        public IEnumerable<SyntaxNode> ExtractedStatements { private set; get; }

        /* Expression to extract in the original code. */
        public SyntaxNode ExtractedExpression { private set; get; }

        public RefactoringType type
        {
            get { return RefactoringType.EXTRACT_METHOD; }
        }

        internal ManualExtractMethodRefactoring(SyntaxNode declaration, SyntaxNode invocation, IEnumerable<SyntaxNode> statements )
        {
            ExtractedMethodDeclaration = declaration;
            ExtractMethodInvocation = invocation;
            ExtractedStatements = statements;
            ExtractedExpression = null;
        }

        internal ManualExtractMethodRefactoring(SyntaxNode declaration, SyntaxNode invocation, SyntaxNode expression)
        {
            ExtractedMethodDeclaration = declaration;
            ExtractMethodInvocation = invocation;
            ExtractedExpression = expression;
            ExtractedStatements = null;
        }

        /* Output the information of a detected extract method refactoring for testing and log purposes.*/
        public string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Manual Extract Method Refactoring:");
            sb.AppendLine("Extracted Method Declaration:\n" + ExtractedMethodDeclaration);
            sb.AppendLine("Extracted Method Invocation:\n" + ExtractMethodInvocation);
            if (ExtractedStatements == null)
                sb.AppendLine("Extracted Expression:\n" + ExtractedExpression);
            else
                sb.AppendLine("Extracted Statements:\n" + StringUtil.ConcatenateAll("\n", ExtractedStatements.Select(s => s.GetText())));
            return sb.ToString();
        }

        public void MapToDocuments(IDocument before, IDocument after)
        {
            var nodeAnalyzer = AnalyzerFactory.GetSyntaxNodeAnalyzer();

            // Map extracted method declaration to the after document.
            nodeAnalyzer.SetSyntaxNode(ExtractedMethodDeclaration);
            ExtractedMethodDeclaration = nodeAnalyzer.MapToAnotherDocument(after);

            // Map the invocation of extracted method to the after document.
            nodeAnalyzer.SetSyntaxNode(ExtractMethodInvocation);
            ExtractMethodInvocation = nodeAnalyzer.MapToAnotherDocument(after);

            // Map the extracted expression to the before document.
            if(ExtractedExpression != null)
            {
                nodeAnalyzer.SetSyntaxNode(ExtractedExpression);
                ExtractedExpression = nodeAnalyzer.MapToAnotherDocument(before);
            }

            // Map the extracted statements to the before document.
            if(ExtractedStatements != null)
            {
                var nodesAnalyzer = AnalyzerFactory.GetSyntaxNodesAnalyzer();
                nodesAnalyzer.SetSyntaxNodes(ExtractedStatements);
                ExtractedStatements = nodesAnalyzer.MapToAnotherDocument(before);
            }
        }
    }

    internal class ManualRenameRefactoring : IManualRenameRefactoring
    {
        public RefactoringType type
        {
            get { return RefactoringType.RENAME; }
        }

        public void MapToDocuments(IDocument before, IDocument after)
        {
            throw new NotImplementedException();
        }
    }
}
