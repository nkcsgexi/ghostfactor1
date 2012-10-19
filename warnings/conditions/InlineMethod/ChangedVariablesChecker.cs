using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.analyzer;
using warnings.refactoring;

namespace warnings.conditions
{
    partial class InlineMethodConditionCheckersList
    {
        private class ChangedVariableValuesChecker : InlineMethodConditionsChecker
        {
            public override ICodeIssueComputer CheckInlineMethodCondition(IDocument before, IDocument after, 
                IInlineMethodRefactoring refactoring)
            {
                // Get the out going symbols before the method is inlined.
                var writtenSymbolsBeforeInline = ConditionCheckersUtils.GetFlowOutData(GetStatementEnclosingInvocation
                    (refactoring.InlinedMethodInvocation), before);

                // Get the out going symbols after the method is inlined.
                var writtenSymbolsAfterInline = ConditionCheckersUtils.GetFlowOutData(refactoring.InlinedStatements, after);
                
                // Calculate the symbols that are added by inlining method.
                var addedSymbols = ConditionCheckersUtils.GetSymbolListExceptByName(writtenSymbolsAfterInline, 
                    writtenSymbolsBeforeInline);

                // Calculate the symbols that are removed by inlining method.
                var missingSymbols = ConditionCheckersUtils.GetSymbolListExceptByName(writtenSymbolsBeforeInline,
                    writtenSymbolsAfterInline);
                
                if(addedSymbols.Any() || missingSymbols.Any())
                {
                    return new ModifiedFlowOutData(addedSymbols, missingSymbols);
                }
                return new NullCodeIssueComputer();
            }

            /* Get the statement that is enclosing the given method invocation. */
            private SyntaxNode GetStatementEnclosingInvocation(SyntaxNode invocation)
            {
                SyntaxNode parent;
                for (parent = invocation; parent != null && !(parent is StatementSyntax); parent = parent.Parent);
                return parent;
            }

            private class ModifiedFlowOutData : ICodeIssueComputer
            {
                internal ModifiedFlowOutData(IEnumerable<ISymbol> addedSymbols, 
                    IEnumerable<ISymbol> missingSymbols)
                {
                    throw new NotImplementedException();
                }

                public bool Equals(ICodeIssueComputer other)
                {
                    throw new NotImplementedException();
                }

                public RefactoringType RefactoringType
                {
                    get { return RefactoringType.INLINE_METHOD; }
                }

                public IEnumerable<CodeIssue> ComputeCodeIssues(IDocument document, SyntaxNode node)
                {
                   
                    throw new NotImplementedException();
                }

                private class ModifiedFlowOutDataFix : ICodeAction
                {
                    public CodeActionEdit GetEdit(CancellationToken cancellationToken = new CancellationToken())
                    {
                        throw new NotImplementedException();
                    }

                    public ImageSource Icon
                    {
                        get { throw new NotImplementedException(); }
                    }

                    public string Description
                    {
                        get { throw new NotImplementedException(); }
                    }
                }

            }

        }
    }
}
