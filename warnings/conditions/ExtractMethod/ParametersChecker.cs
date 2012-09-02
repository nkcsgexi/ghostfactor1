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
    internal class ParametersChecker : ExtractMethodConditionChecker
    {
        public override ICheckingResult CheckCondition(IDocument before, IDocument after, IManualRefactoring input)
        {
            ManualExtractMethodRefactoring manual = (ManualExtractMethodRefactoring) input;
            MethodDeclarationSyntax declaration = (MethodDeclarationSyntax) manual.ExtractedMethodDeclaration;
            InvocationExpressionSyntax invocation = (InvocationExpressionSyntax) manual.ExtractMethodInvocation;
            IEnumerable<SyntaxNode> statements = manual.ExtractedStatements;

            // Calculate the needed parameters. 
            var statementsDataFlowAnalyzer = AnalyzerFactory.GetStatementsDataFlowAnalyzer();
            statementsDataFlowAnalyzer.SetDocument(before);
            statementsDataFlowAnalyzer.SetStatements(statements);
            var needed = statementsDataFlowAnalyzer.GetFlowInData();

            // Calculate the used symbols in the method invocation.
            var expressionDataFlowAnalyzer = AnalyzerFactory.GetExpressionDataFlowAnalyzer();
            expressionDataFlowAnalyzer.SetDocument(after);
            expressionDataFlowAnalyzer.SetExpression(invocation);
            var used = expressionDataFlowAnalyzer.GetFlowIn();


            // TODO: comapare the difference between needed and used. 

            return null;
        }
    }

    internal class ParameterCheckResult : ExtractMethodConditionCheckingResult
    {
        public override bool HasProblem()
        {
            throw new NotImplementedException();
        }

        public override string GetProblemDescription()
        {
            throw new NotImplementedException();
        }
    }
}
