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
using warnings.refactoring;
using warnings.retriever;
using warnings.util;

namespace warnings.conditions
{
    /* This checker is checking whether the extracted method has taken enough or more than enough parameters than actual need. */
    internal class ParametersChecker : ExtractMethodConditionChecker
    {
        private Logger logger = NLoggerUtil.getNLogger(typeof (ParametersChecker));

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
              
            // Calculate the used symbols in the method declaration.
            var expressionDataFlowAnalyzer = AnalyzerFactory.GetExpressionDataFlowAnalyzer();
            expressionDataFlowAnalyzer.SetDocument(after);
            expressionDataFlowAnalyzer.SetExpression(invocation);
            var used = expressionDataFlowAnalyzer.GetFlowInData();

            // Calculate the missing symbols and the extra symbols, also, trivial to show 'this' so remove.
            var missing = RemoveThisSymbol(GetSymbolListExceptByName(needed, used));
         
            // if missing is not empty, then some parameters are needed. 
            if (missing.Any())
            {
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

        private class ParameterCheckingCodeIssueComputer : ICodeIssueComputer
        {
            private SyntaxNode declaration;
            private IEnumerable<string> parameters;

            public ParameterCheckingCodeIssueComputer(SyntaxNode declaration, IEnumerable<string> parameters)
            {
                this.declaration = declaration;
                this.parameters = parameters;
            }

            public IEnumerable<CodeIssue> ComputeCodeIssues(IDocument document, SyntaxNode node)
            {
                if(node.Kind == SyntaxKind.InvocationExpression)
                {
                    var retriever = RetrieverFactory.GetMethodInvocationRetriever();
                    retriever.SetDocument(document);
                    retriever.SetMethodDeclaration(declaration);
                    var invocations = retriever.GetInvocations();
                    if(invocations.Any(i => i.Span.Equals(node)))
                    {
                        yield return new CodeIssue(CodeIssue.Severity.Warning, node.Span,
                            "Missing parameters " + StringUtil.ConcatenateAll(",", parameters));
                    }
                }
            }
        }


    }  
}
