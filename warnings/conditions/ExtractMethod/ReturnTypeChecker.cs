﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.analyzer;
using warnings.refactoring;
using warnings.retriever;
using warnings.util;

namespace warnings.conditions
{
    /* Checker for whether the extracted method returns the right value. */

    internal class ReturnTypeChecker : ExtractMethodConditionChecker
    {
        private Logger logger = NLoggerUtil.getNLogger(typeof (ReturnTypeChecker));

        protected override ICodeIssueComputer CheckCondition(IDocument before, IDocument after, IManualExtractMethodRefactoring input)
        {
            // Calculate the outflow data
            IEnumerable<ISymbol> flowOuts;
            if (input.ExtractedStatements != null)
                flowOuts = GetFlowOutData(input.ExtractedStatements, before);
            else
                flowOuts = GetFlowOutData(input.ExtractedExpression, before);

            // Get the returning data of the return statements.
            var delaration = input.ExtractedMethodDeclaration;
            var methodAnalyzer = AnalyzerFactory.GetMethodAnalyzer();
            methodAnalyzer.SetMethodDeclaration(delaration);

            // Get the returning data in the return statements of the extracted method, also log them.
            var returningData = GetMethodReturningData(methodAnalyzer, after);

            // Missing symbols that are in the flow out before but not in the returning data.
            // Remove this symbol.
            var missing = RemoveThisSymbol(GetSymbolListExceptByName(flowOuts, returningData));

            if(missing.Any())
            {
                return new ReturnTypeCheckingResult(input.ExtractedMethodDeclaration, missing.Select(s => s.Name));
            }
            else
            {
                return new NullCodeIssueComputer();
            }
        }

        private IEnumerable<ISymbol> GetFlowOutData(IEnumerable<SyntaxNode> statements, IDocument document)
        {
            var statementsDataFlowAnalyzer = AnalyzerFactory.GetStatementsDataFlowAnalyzer();
            statementsDataFlowAnalyzer.SetDocument(document);
            statementsDataFlowAnalyzer.SetStatements(statements);
            var flowOuts = statementsDataFlowAnalyzer.GetFlowOutData();
            logger.Info("Statements Flowing Out Data: " + StringUtil.ConcatenateAll(", ", flowOuts.Select(s => s.Name)));
            return flowOuts;
        }

        private IEnumerable<ISymbol> GetFlowOutData(SyntaxNode expression, IDocument document)
        {
            var expressionDataFlowAnalyzer = AnalyzerFactory.GetExpressionDataFlowAnalyzer();
            expressionDataFlowAnalyzer.SetDocument(document);
            expressionDataFlowAnalyzer.SetExpression(expression);
            var flowOuts = expressionDataFlowAnalyzer.GetFlowOutData();
            logger.Info("Expression Flowing Out Data: " + StringUtil.ConcatenateAll(", ", flowOuts.Select(s => s.Name)));
            return flowOuts;
        }

        private IEnumerable<ISymbol> GetMethodReturningData(IMethodAnalyzer methodAnalyzer, IDocument document)
        {
            
            // The returning data from the return statements is initiated as empty.
            var returningData = Enumerable.Empty<ISymbol>();

            // If having return statement, then retuning data could be not empty.
            if(methodAnalyzer.HasReturnStatement())
            {
                // Get all the return statements.
                var return_statements = methodAnalyzer.GetReturnStatements();

                // Get the data flow analyzer for statements.
                var dataFlowAnalyzer = AnalyzerFactory.GetStatementsDataFlowAnalyzer();

                // Set the document to be the after one.
                dataFlowAnalyzer.SetDocument(document);

                // A list containing one statement.
                var stats = new List<SyntaxNode>();

                foreach (var s in return_statements)
                {
                    // make the list empty first.
                    stats.Clear();

                    // Analyze one single return statement at each iteration
                    stats.Add(s);
                    dataFlowAnalyzer.SetStatements(stats);

                    // Combining with the current result.
                    returningData = returningData.Union(dataFlowAnalyzer.GetFlowInData());
                }
            }
            logger.Info("Returning Data: " + StringUtil.ConcatenateAll(", ", returningData.Select(s => s.Name)));
            return returningData;
        }

        private class ReturnTypeCheckingResult : ICodeIssueComputer
        {
            private IEnumerable<string> returns;
            private SyntaxNode declaration;

            public ReturnTypeCheckingResult(SyntaxNode declaration, IEnumerable<string> returns)
            {
                this.declaration = declaration;
                this.returns = returns;
            }
            public IEnumerable<CodeIssue> ComputeCodeIssues(IDocument document, SyntaxNode node)
            {
                if (node.Kind == SyntaxKind.InvocationExpression)
                {
                    var retriever = RetrieverFactory.GetMethodInvocationRetriever();
                    retriever.SetDocument(document);
                    retriever.SetMethodDeclaration(declaration);
                    var invocations = retriever.GetInvocations();
                    if (invocations.Any(i => i.Span.Equals(node.Span)))
                    {
                        yield return new CodeIssue(CodeIssue.Severity.Warning, node.Span,
                            "Missing return values: " + StringUtil.ConcatenateAll(",", returns));
                    }
                }
            }
        }
    }

    

}
