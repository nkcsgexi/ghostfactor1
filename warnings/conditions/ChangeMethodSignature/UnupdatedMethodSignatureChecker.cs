using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Services;
using warnings.refactoring;

namespace warnings.conditions
{
    class UnupdatedMethodSignatureChecker : IRefactoringConditionChecker
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

        public ICheckingResult CheckCondition(IDocument before, IDocument after, IManualRefactoring input)
        {
            throw new NotImplementedException();
        }
    }
}
