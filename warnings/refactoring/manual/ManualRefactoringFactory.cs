﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using warnings.analyzer;
using warnings.util;

namespace warnings.refactoring
{
    public class ManualRefactoringFactory
    {
        /* Create a manual extract method refactoring that extracts statements. */
        public static IManualExtractMethodRefactoring CreateManualExtractMethodRefactoring(SyntaxNode declaration, 
            SyntaxNode invocation, IEnumerable<SyntaxNode> statements)
        {
            return new ManualExtractMethodRefactoring(declaration, invocation, statements);
        }

        /* Create a manual extract method refacoting that extracts a expression. */
        public static IManualExtractMethodRefactoring CreateManualExtractMethodRefactoring( SyntaxNode declaration, 
            SyntaxNode invocation, SyntaxNode expression)
        {
            return new ManualExtractMethodRefactoring(declaration, invocation, expression);
        }

        /* 
         * Create a manual rename refactoring, the token (of RefactoringType identifier token) is where the rename is performed on,
         * the new name is the name given to the identifier. Token is in the before version. 
         */
        public static IManualRenameRefactoring CreateManualRenameRefactoring(SyntaxNode node, string newName)
        {
            return new ManualRenameRefactoring(node, newName);
        }

        /* Create a manual change method signature refactoring. */
        public static IChangeMethodSignatureRefactoring CreateManualChangeMethodSignatureRefactoring
            (SyntaxNode afterMethod, List<Tuple<int, int>> parametersMap)
        {
            return new ChangeMethodSignatureRefactoring(afterMethod, parametersMap);
        }

        /* Create an instance of manual inline method refactoring. */
        public static IInlineMethodRefactoring CreateManualInlineMethodRefactoring(SyntaxNode methodBefore, SyntaxNode methodAfter, SyntaxNode methodInlined, 
            SyntaxNode inlinedMethodInvocation,IEnumerable<SyntaxNode> inlinedStatements)
        {
            return new InlineMethodRefactoring(methodBefore, methodAfter, methodInlined, inlinedMethodInvocation, inlinedStatements);
        }

        private class ManualRenameRefactoring : IManualRenameRefactoring
        {
            private readonly string newName;
            private readonly SyntaxNode node;

            public ManualRenameRefactoring(SyntaxNode node, string newName)
            {
                this.node = node;
                this.newName = newName;
            }

            public RefactoringType RefactoringType
            {
                get { return RefactoringType.RENAME; }
            }

            public void MapToDocuments(IDocument before, IDocument after)
            {
                throw new NotImplementedException();
            }

            public SyntaxNode GetIssuedNode()
            {
                return node;
            }
        }

        /* Containing all the information about the extract method information. */
        private class ManualExtractMethodRefactoring : IManualExtractMethodRefactoring
        {
            /* Method declaration node of the extracted method. */
            public SyntaxNode ExtractedMethodDeclaration { private set; get; }

            /* Method invocation node where the extracted method is invoked. */
            public SyntaxNode ExtractMethodInvocation { private set; get; }

            /* Statements to extract in the original code. */
            public IEnumerable<SyntaxNode> ExtractedStatements { private set; get; }

            /* Expression to extract in the original code. */
            public SyntaxNode ExtractedExpression { private set; get; }

            public RefactoringType RefactoringType
            {
                get { return RefactoringType.EXTRACT_METHOD; }
            }

            internal ManualExtractMethodRefactoring(SyntaxNode declaration, SyntaxNode invocation,
                                                    IEnumerable<SyntaxNode> statements)
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
                    sb.AppendLine("Extracted Statements:\n" +
                        StringUtil.ConcatenateAll("\n", ExtractedStatements.Select(s => s.GetText())));
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
                if (ExtractedExpression != null)
                {
                    nodeAnalyzer.SetSyntaxNode(ExtractedExpression);
                    ExtractedExpression = nodeAnalyzer.MapToAnotherDocument(before);
                }

                // Map the extracted statements to the before document.
                if (ExtractedStatements != null)
                {
                    var nodesAnalyzer = AnalyzerFactory.GetSyntaxNodesAnalyzer();
                    nodesAnalyzer.SetSyntaxNodes(ExtractedStatements);
                    ExtractedStatements = nodesAnalyzer.MapToAnotherDocument(before);
                }
            }

            public SyntaxNode GetIssuedNode()
            {
                return ExtractMethodInvocation;
            }
        }

        /* Describing a change method signature refactoring. */
        private class ChangeMethodSignatureRefactoring : IChangeMethodSignatureRefactoring
        {

            public ChangeMethodSignatureRefactoring(SyntaxNode ChangedMethodDeclaration,
                List<Tuple<int, int>> ParametersMap)
            {
                this.ChangedMethodDeclaration = ChangedMethodDeclaration;
                this.ParametersMap = ParametersMap;
            }

            public RefactoringType RefactoringType
            {
                get { return RefactoringType.CHANGE_METHOD_SIGNATURE; }
            }

            public void MapToDocuments(IDocument before, IDocument after)
            {
                var analyzer = AnalyzerFactory.GetSyntaxNodeAnalyzer();
                analyzer.SetSyntaxNode(ChangedMethodDeclaration);
                ChangedMethodDeclaration = analyzer.MapToAnotherDocument(after);
            }

            public SyntaxNode GetIssuedNode()
            {
                throw new NotImplementedException();
            }

            /* Method declaration after the changed declaration, should map to the after document. */
            public SyntaxNode ChangedMethodDeclaration { get; private set; }

            /* Parameters map from before version of the method and the after version of the methods. */
            public List<Tuple<int, int>> ParametersMap { get; private set; }
        }

        /* Describing a inline method refactoring. */
        private class InlineMethodRefactoring : IInlineMethodRefactoring
        {
            internal InlineMethodRefactoring(SyntaxNode CallerMethodBefore, SyntaxNode CallerMethodAfter,
               SyntaxNode InlinedMethod, SyntaxNode InlinedMethodInvocation, IEnumerable<SyntaxNode> InlinedStatements)
            {
                this.CallerMethodAfter = CallerMethodAfter;
                this.CallerMethodBefore = CallerMethodBefore;
                this.InlinedMethod = InlinedMethod;
                this.InlinedStatements = InlinedStatements;
                this.InlinedMethodInvocation = InlinedMethodInvocation;
            }

            public RefactoringType RefactoringType
            {
                get { return RefactoringType.INLINE_METHOD; }
            }

            public void MapToDocuments(IDocument before, IDocument after)
            {
                throw new NotImplementedException();
            }

            public SyntaxNode GetIssuedNode()
            {
                throw new NotImplementedException();
            }

            public SyntaxNode CallerMethodBefore { get; private set; }

            public SyntaxNode CallerMethodAfter { get; private set; }

            public SyntaxNode InlinedMethod { get; private set; }

            public SyntaxNode InlinedMethodInvocation { get; private set; }

            public IEnumerable<SyntaxNode> InlinedStatements { get; private set; }
        }
    }
}
