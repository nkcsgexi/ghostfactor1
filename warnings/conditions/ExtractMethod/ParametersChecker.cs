using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using warnings.analyzer;
using warnings.refactoring;

namespace warnings.conditions
{
    /* This checker is checking whether the extracted method has taken enough or more than enough parameters than actual need. */
    internal class ParametersChecker : ExtractMethodConditionChecker
    {
        protected override ExtractMethodConditionCheckingResult CheckCondition(IDocument before, IDocument after, 
            IManualExtractMethodRefactoring input)
        {
            InvocationExpressionSyntax invocation = (InvocationExpressionSyntax) input.ExtractMethodInvocation;
            IEnumerable<SyntaxNode> statements = input.ExtractedStatements;

            // Calculate the needed parameters. 
            var statementsDataFlowAnalyzer = AnalyzerFactory.GetStatementsDataFlowAnalyzer();
            statementsDataFlowAnalyzer.SetDocument(before);
            statementsDataFlowAnalyzer.SetStatements(statements);
            var needed = statementsDataFlowAnalyzer.GetFlowInData();

            // Calculate the used symbols in the method invocation.
            var expressionDataFlowAnalyzer = AnalyzerFactory.GetExpressionDataFlowAnalyzer();
            expressionDataFlowAnalyzer.SetDocument(after);
            expressionDataFlowAnalyzer.SetExpression(invocation);
            var used = expressionDataFlowAnalyzer.GetFlowInData();

            // Calculate the missing symbols and the extra symbols. 
            var missing = needed.Except(used);
            var extra = used.Except(needed);

            // Build the description of missing variables.
            var sb = new StringBuilder();
            var hasProblem = false;
            if (missing.Any())
            {
                hasProblem = true;
                sb.Append("Missing Variables:");
                foreach (var symbol in missing)
                {
                    sb.Append(symbol.Name + " ");
                }
            }

            // Append the description of extra variables.
            if (extra.Any())
            {
                hasProblem = true;
                sb.AppendLine();
                sb.Append("Redundant Variables: ");
                foreach (var symbol in extra)
                {
                    sb.Append(symbol.Name + " ");
                }
            }

            // Return the checking result.
            return new ParameterCheckResult(hasProblem, sb.ToString());
        }
    }

    /* The check result of the parameter checkings of newly extracted method. */
    internal class ParameterCheckResult : ExtractMethodConditionCheckingResult
    {
        private readonly string description;
        private readonly bool hasProblem;

        internal ParameterCheckResult(bool hasProblem, string description)
        {
            this.hasProblem = hasProblem;
            this.description = description;
        }

        public override bool HasProblem()
        {
            return hasProblem;
        }

        public override string GetProblemDescription()
        {
            return description;
        }
    }
}
