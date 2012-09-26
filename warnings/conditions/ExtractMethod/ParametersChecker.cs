using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.analyzer;
using warnings.analyzer.comparators;
using warnings.refactoring;
using warnings.retriever;
using warnings.util;

namespace warnings.conditions
{
    /* This checker is checking whether the extracted method has taken enough or more than enough parameters than actual need. */
    internal class ParametersChecker : ExtractMethodConditionChecker
    {
        private Logger logger = NLoggerUtil.GetNLogger(typeof (ParametersChecker));

        protected override ICodeIssueComputer CheckCondition(IDocument before, IDocument after, 
            IManualExtractMethodRefactoring input)
        {
            InvocationExpressionSyntax invocation = (InvocationExpressionSyntax) input.ExtractMethodInvocation;
           
            // Calculate the needed parameters, depending on what to extract.
            IEnumerable<ISymbol> needed;
            if (input.ExtractedStatements != null)
                needed = GetFlowInData(input.ExtractedStatements, before);
            else
                needed = GetFlowInData(input.ExtractedExpression, before);

            // Logging the needed parameters.
            logger.Info("Needed parameters: " + StringUtil.ConcatenateAll(",", needed.Select(s => s.Name)));

            // Calculate the used symbols in the method declaration.
            var expressionDataFlowAnalyzer = AnalyzerFactory.GetExpressionDataFlowAnalyzer();
            expressionDataFlowAnalyzer.SetDocument(after);
            expressionDataFlowAnalyzer.SetExpression(invocation);
            var used = expressionDataFlowAnalyzer.GetFlowInData();

            // Logging the used parameters.
            logger.Info("Used parameters: " + StringUtil.ConcatenateAll(",", used.Select(s => s.Name)));

            // Calculate the missing symbols and the extra symbols, also, trivial to show 'this' so remove.
            var missing = RemoveThisSymbol(GetSymbolListExceptByName(needed, used));
         
            // if missing is not empty, then some parameters are needed. 
            if (missing.Any())
            {
                logger.Info("Missing Parameters Issue Found.");
                return new ParameterCheckingCodeIssueComputer(input.ExtractedMethodDeclaration, missing.Select(s => s.Name));   
            }
            else
            {
                // Otherwise, return no problem.
                return new NullCodeIssueComputer();
            }
        }

        /* Get needed parameters if extracting statements. */
        IEnumerable<ISymbol> GetFlowInData(IEnumerable<SyntaxNode> statements, IDocument document)
        {
            var statementsDataFlowAnalyzer = AnalyzerFactory.GetStatementsDataFlowAnalyzer();
            statementsDataFlowAnalyzer.SetDocument(document);
            statementsDataFlowAnalyzer.SetStatements(statements);
            return statementsDataFlowAnalyzer.GetFlowInData(); ;
        }

        /* Get needed parameters if extracting an expression. */
        IEnumerable<ISymbol> GetFlowInData(SyntaxNode expression, IDocument document)
        {
            var expressionDataFlowAnalyzer = AnalyzerFactory.GetExpressionDataFlowAnalyzer();
            expressionDataFlowAnalyzer.SetDocument(document);
            expressionDataFlowAnalyzer.SetExpression(expression);
            return expressionDataFlowAnalyzer.GetFlowInData();
        }

        /* Code issue computer for parameter checking results. */
        private class ParameterCheckingCodeIssueComputer : ICodeIssueComputer
        {
            private readonly Logger logger;
            
            /* Declaration of the extracted method. */
            private SyntaxNode declaration;

            /* The missing parameters' names. */
            private IEnumerable<string> parameters;

            public ParameterCheckingCodeIssueComputer(SyntaxNode declaration, IEnumerable<string> parameters)
            {
                this.declaration = declaration;
                this.parameters = parameters;
                this.logger = NLoggerUtil.GetNLogger(typeof(ParameterCheckingCodeIssueComputer));
            }

            public IEnumerable<CodeIssue> ComputeCodeIssues(IDocument document, SyntaxNode node)
            {
                // If the node is not method invocation, does not proceed.
                if(node.Kind == SyntaxKind.InvocationExpression)
                {
                    // Find all invocations of the extracted method.
                    var retriever = RetrieverFactory.GetMethodInvocationRetriever();
                    retriever.SetDocument(document);
                    retriever.SetMethodDeclaration(declaration);
                    var invocations = retriever.GetInvocations();

                    // If the given node is one of these invocations, return a new issue.
                    if(invocations.Any(i => i.Span.Equals(node.Span)))
                    {
                        yield return new CodeIssue(CodeIssue.Severity.Warning, node.Span,
                            "Missing parameters " + StringUtil.ConcatenateAll(",", parameters));
                    }
                }
            }

            public bool Equals(ICodeIssueComputer o)
            {
                // If the other is not in the same type, return false
                if(o is ParameterCheckingCodeIssueComputer)
                {
                    var other = (ParameterCheckingCodeIssueComputer) o;
                    var methodsComparator = new MethodsComparator();
                    var stringEnumerablesComparator = new StringEnumerablesComparator();

                    // If the method declarations are equal to each other.
                    return methodsComparator.Compare(declaration, other.declaration) == 0 &&
                        // Also the contained parameter names are equal to each other, return true;
                        stringEnumerablesComparator.Compare(parameters, other.parameters) == 0;
                }
                return false;
            }
        }


    }  
}
