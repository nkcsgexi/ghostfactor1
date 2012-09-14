using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Services;
using warnings.refactoring;

namespace warnings.conditions
{
    class ChangeMethodSignatureConditionsList : IRefactoringConditionsList
    {
        private static IRefactoringConditionsList list;
        public static IRefactoringConditionsList GetInstance()
        {
            if(list == null)
                list = new ChangeMethodSignatureConditionsList();
            return list;
        }
    
        public RefactoringType type
        {
            get { return RefactoringType.CHANGE_METHOD_SIGNATURE; }
        }

        public IEnumerable<ICheckingResult> CheckAllConditions(IDocument before, IDocument after, IManualRefactoring input)
        {
            var results = new List<ICheckingResult>();

            results.Add(UnupdatedMethodSignatureChecker.GetInstance().CheckCondition(before, after, input));

            return results.AsEnumerable();
        }
    }
}
