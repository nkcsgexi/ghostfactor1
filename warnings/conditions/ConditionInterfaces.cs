using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
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
        ICheckingResult CheckCondition(IDocument before, IDocument after, IManualRefactoring input);  
    }

    /* interface that containing checkings for all the conditions of a refactoring type. */
    public interface IRefactoringConditionsList : IHasRefactoringType
    {
        IEnumerable<ICheckingResult> CheckAllConditions(IDocument before, IDocument after, IManualRefactoring input);
    }


    /* Refactoring conditions for a specific refactoring type is stored in.*/
    public abstract class RefactoringConditionsList : IRefactoringConditionsList
    {
        /* suppose to return all the condition checkers for this specific refactoring. */
        public IEnumerable<ICheckingResult> CheckAllConditions(IDocument before, IDocument after, IManualRefactoring input)
        {
            List<ICheckingResult> results = new List<ICheckingResult>();
            foreach (var checker in GetAllConditionCheckers())
            {
                results.Add(checker.CheckCondition(before, after, input));
            }
            return results.AsEnumerable();
        }

        protected abstract IEnumerable<IRefactoringConditionChecker> GetAllConditionCheckers();
        public abstract RefactoringType type { get; }
    }

    /* After checking the conditon, the condtion shall return this as a result. */
    public interface ICheckingResult : IHasRefactoringType
    {
        bool HasProblem();
        string GetProblemDescription();
    }



 
}
