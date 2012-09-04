using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using warnings.analyzer;
using warnings.refactoring;
using warnings.util;

namespace warnings.conditions
{
    /* This checker is checking whether the extracted method has taken enough or more than enough parameters than actual need. */
    internal class ParametersChecker : ExtractMethodConditionChecker
    {
        protected override ExtractMethodConditionCheckingResult CheckCondition(IDocument before, IDocument after, 
            IManualExtractMethodRefactoring input)
        {
            InvocationExpressionSyntax invocation = (InvocationExpressionSyntax) input.ExtractMethodInvocation;
           
            // Calculate the needed parameters, depending on what to extract.
            IEnumerable<ISymbol> needed;
            if (input.ExtractedStatements != null)
                needed = GetFlowInData(input.ExtractedStatements, before);
            else
                needed = GetFlowInData(input.ExtractedExpression, before);
              
            // Calculate the used symbols in the method invocation.
            var expressionDataFlowAnalyzer = AnalyzerFactory.GetExpressionDataFlowAnalyzer();
            expressionDataFlowAnalyzer.SetDocument(after);
            expressionDataFlowAnalyzer.SetExpression(invocation);
            var used = expressionDataFlowAnalyzer.GetFlowInData();

            // Calculate the missing symbols and the extra symbols, also, trivial to show 'this' so remove.
            var missing = RemoveThisSymbol(GetSymbolListExceptByName(needed, used));
         
            // if missing is not empty, then some parameters are needed. 
            if (missing.Any())
            {
                return new ParameterCheckingResult(true, missing.Select(s => s.Name));   
            }
            else
            {
                // Otherwise, return no problem.
                return new ParameterCheckingResult(false);
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
    }

    /* The check result of the parameter checkings of newly extracted method. */
    internal class ParameterCheckingResult : ExtractMethodConditionCheckingResult
    {
        private readonly string description;
        private readonly bool hasProblem;
        private readonly IEnumerable<string> missingParaNames;

        internal ParameterCheckingResult(bool hasProblem, IEnumerable<string> missingParaNames = null )
        {
            this.hasProblem = hasProblem;
            if(hasProblem)
            {
                this.missingParaNames = missingParaNames;
                description = "Missing Parameters: " + StringUtil.ConcatenateAll(", ", missingParaNames);
            }
        }

        public override bool HasProblem()
        {
            return hasProblem;
        }

        public override string GetProblemDescription()
        {
            return description;
        }

        public IEnumerable<String> GetMissingParametersNames()
        {
            return missingParaNames;
        }
    }
}
