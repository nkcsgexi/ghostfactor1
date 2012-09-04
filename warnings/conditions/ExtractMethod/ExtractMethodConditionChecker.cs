using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using warnings.refactoring;

namespace warnings.conditions
{
    /* All the condition checkers for extract method should implement this. */
    abstract class ExtractMethodConditionChecker : IRefactoringConditionChecker
    {
        public RefactoringType type
        {
            get { return RefactoringType.EXTRACT_METHOD; }
        }

        public ICheckingResult CheckCondition(IDocument before, IDocument after, IManualRefactoring input)
        {
            return CheckCondition(before, after, (IManualExtractMethodRefactoring)input);
        }

        /* Except one symbol list from the other by symbol name. */
        protected IEnumerable<ISymbol> GetSymbolListExceptByName(IEnumerable<ISymbol> original, IEnumerable<ISymbol> except)
        {
            var result = new List<ISymbol>();
            foreach (ISymbol o in original)
            {
                var name = o.Name;
                if (!except.Any(e => e.Name.Equals(name)))
                {
                    result.Add(o);
                }
            }
            return result.AsEnumerable();
        }

        /* Remove 'this' symbol in a list of symbols. */
        protected IEnumerable<ISymbol> RemoveThisSymbol(IEnumerable<ISymbol> original)
        {
            return original.Where(s => !s.Name.Equals("this"));
        }


        protected abstract ExtractMethodConditionCheckingResult CheckCondition(IDocument before, IDocument after,
                                                                   IManualExtractMethodRefactoring input);
    }


    /* All checking result for extract method shall derive from this. */
    abstract class ExtractMethodConditionCheckingResult : ICheckingResult
    {
        public RefactoringType type
        {
            get { return RefactoringType.EXTRACT_METHOD; }
        }

        public abstract bool HasProblem();
        public abstract string GetProblemDescription();
    }

 

    /* Condition list for extract method. */
    internal class ExtractMethodConditionsList : RefactoringConditionsList
    {
        private static Lazy<ExtractMethodConditionsList> instance = new Lazy<ExtractMethodConditionsList>();

        private ExtractMethodConditionsList()
        {
        }

        public static IRefactoringConditionsList GetInstance()
        {
            if(instance.IsValueCreated)
                return instance.Value;
            return new ExtractMethodConditionsList();
        }

        protected override IEnumerable<IRefactoringConditionChecker> GetAllConditionCheckers()
        {
            List<IRefactoringConditionChecker> checkers = new List<IRefactoringConditionChecker>();
            checkers.Add(new ParametersChecker());
            checkers.Add(new ReturnTypeChecker());
            return checkers.AsEnumerable();
        }

        public override RefactoringType type
        {
            get { return RefactoringType.EXTRACT_METHOD; }
        }
    }

}
