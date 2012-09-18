using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.quickfix;
using warnings.refactoring;

namespace warnings.conditions
{
    /* The interface that can be queried about refactoring type. */
    public interface IHasRefactoringType
    {
        RefactoringType type { get; }
    }

    /* All refactoring conditions should be derived from this interface. */
    public interface IRefactoringConditionChecker : IHasRefactoringType
    {
        ICodeIssueComputer CheckCondition(IDocument before, IDocument after, IManualRefactoring input);  
    }

    /* interface that containing checkings for all the conditions of a refactoring type. */
    public interface IRefactoringConditionsList : IHasRefactoringType
    {
        IEnumerable<ICodeIssueComputer> CheckAllConditions(IDocument before, IDocument after, IManualRefactoring input);
    }

    /* Refactoring conditions for a specific refactoring type is stored in.*/
    public abstract class RefactoringConditionsList : IRefactoringConditionsList
    {
        /* suppose to return all the condition checkers for this specific refactoring. */
        public IEnumerable<ICodeIssueComputer> CheckAllConditions(IDocument before, IDocument after, IManualRefactoring input)
        {
            List<ICodeIssueComputer> results = new List<ICodeIssueComputer>();
            foreach (var checker in GetAllConditionCheckers())
            {
                results.Add(checker.CheckCondition(before, after, input));
            }
            return results.AsEnumerable();
        }

        protected abstract IEnumerable<IRefactoringConditionChecker> GetAllConditionCheckers();
        public abstract RefactoringType type { get; }
    }

    /*
     * This interface is used returning values for condition checkers. It is a convenient way of computing code issues. 
     */
    public interface ICodeIssueComputer
    {
        IEnumerable<CodeIssue> ComputeCodeIssues(IDocument document, SyntaxNode node);
    }

    /* The null code issue computer return no code issue at any time. */
    public class NullCodeIssueComputer : ICodeIssueComputer
    {
        public IEnumerable<CodeIssue> ComputeCodeIssues(IDocument document, SyntaxNode node)
        {
            return Enumerable.Empty<CodeIssue>();
        }
    }
}
