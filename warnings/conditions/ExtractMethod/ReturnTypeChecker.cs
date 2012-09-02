using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using warnings.analyzer;
using warnings.refactoring;

namespace warnings.conditions
{
    /* Checker for whether the extracted method returns the right value. */
    class ReturnTypeChecker : ExtractMethodConditionChecker
    {
        protected override ExtractMethodConditionCheckingResult CheckCondition(IDocument before, IDocument after, IManualExtractMethodRefactoring input)
        {
            // Calculate the outflow data from the original statements.
            var statements = input.ExtractedStatements;
            var statementsDataFlowAnalyzer = AnalyzerFactory.GetStatementsDataFlowAnalyzer();
            statementsDataFlowAnalyzer.SetDocument(before);
            statementsDataFlowAnalyzer.SetStatements(statements);
            var flowOuts = statementsDataFlowAnalyzer.GetFlowOutData();

            // Get the returning data of the return statements.
            var delaration = input.ExtractedMethodDeclaration;
            var methodAnalyzer = AnalyzerFactory.GetMethodAnalyzer();
            methodAnalyzer.SetMethodDeclaration(delaration);

            // Get the returning data in the return statements of the extracted method.
            var returningData = GetMethodReturningData(methodAnalyzer, after);

            // Buid up the error message. 
            var sb = new StringBuilder();
            var hasProblem = false;

            // Missing symbols that are in the flow out before but not in the returning data. 
            var missing = flowOuts.Except(returningData);
            if(missing.Any())
            {
                hasProblem = true;
                sb.AppendLine("Missing Return Value: " + CombineSymbolName(missing));
            }

            // No needed symbols that are in the returning value but not in the flow out before.
            var noNeed = returningData.Except(flowOuts);
            if(noNeed.Any())
            {
                hasProblem = true;
                sb.AppendLine("No Needed Return Value: " + CombineSymbolName(noNeed));
            }
            return new ReturnTypeCheckingResult(hasProblem, sb.ToString());
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
            return returningData;
        }

        private string CombineSymbolName(IEnumerable<ISymbol> symbols)
        {
            var sb = new StringBuilder();
            foreach (var symbol in symbols)
            {
                sb.Append(" " + symbol.Name);
            }
            return sb.ToString();
        }

    }


    class ReturnTypeCheckingResult : ExtractMethodConditionCheckingResult
    {
        private string description;
        private bool hasProblem;

        internal ReturnTypeCheckingResult(bool hasProblem, string description)
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
