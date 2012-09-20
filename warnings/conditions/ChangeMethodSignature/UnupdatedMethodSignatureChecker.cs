using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.analyzer;
using warnings.analyzer.comparators;
using warnings.components.search;
using warnings.quickfix;
using warnings.refactoring;
using warnings.retriever;
using warnings.util;

namespace warnings.conditions
{
    internal class UnupdatedMethodSignatureChecker : IRefactoringConditionChecker
    {
        private static IRefactoringConditionChecker instance;

        public static IRefactoringConditionChecker GetInstance()
        {
            if (instance == null)
            {
                instance = new UnupdatedMethodSignatureChecker();
            }
            return instance;
        }

        public RefactoringType type
        {
            get { return RefactoringType.CHANGE_METHOD_SIGNATURE; }
        }

        public ICodeIssueComputer CheckCondition(IDocument before, IDocument after, IManualRefactoring input)
        {
            return new UnchangedMethodInvocationComputer(((IChangeMethodSignatureRefactoring)input).ChangedMethodDeclaration);
        }


        /* The computer for calculating the unchanged method invocations. */
        private class UnchangedMethodInvocationComputer : ICodeIssueComputer
        {
            private readonly SyntaxNode declaration;
           
            public UnchangedMethodInvocationComputer(SyntaxNode declaration)
            {
                this.declaration = declaration;
            }

            public IEnumerable<CodeIssue> ComputeCodeIssues(IDocument document, SyntaxNode node)
            {
                if(node.Kind == SyntaxKind.InvocationExpression)
                {
                    // Retrievers for method invocations.
                    var retriever = RetrieverFactory.GetMethodInvocationRetriever();
                    retriever.SetDocument(document);

                    // Get all the invocations in the current document.
                    retriever.SetMethodDeclaration(declaration);
                    var invocations = retriever.GetInvocations();

                    // If the given node is in the invocations, return a corresponding code issue.
                    if (invocations.Any(n => n.Span.Equals(node.Span)))
                    {
                        yield return new CodeIssue(CodeIssue.Severity.Warning, node.Span,
                            "Method invocation needs update.");
                    }
                }
            }

            public bool Equals(ICodeIssueComputer o)
            {
                if(o is UnchangedMethodInvocationComputer)
                {
                    var comparator = new MethodsComparator();
                    var other = (UnchangedMethodInvocationComputer) o;
                    return comparator.Compare(declaration, other.declaration) == 0;
                }
                return false;
            }
        }
    }
}
