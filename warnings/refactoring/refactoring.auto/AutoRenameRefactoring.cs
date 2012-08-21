using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using warnings.refactoring.attributes;

namespace warnings.refactoring.refactoring.auto
{
    public class AutoRenameRefactoring : IAutoRefactoring, IRefactoring
    {
        private String code;
        private String refactoredCode;
        private IRefactoringAttributes attributes;

        public void setCode(String code)
        {
            this.code = code;
        }

        public bool checkConditions()
        {
            throw new NotImplementedException();
        }

        public void performRefactoring()
        {
            throw new NotImplementedException();
        }

        public string getCodeAfterRefactoring()
        {
            return refactoredCode;
        }

        public void setStart(int start)
        {
            throw new NotImplementedException();
        }

        public void setLength(int length)
        {
            throw new NotImplementedException();
        }

        public RefactoringType getRefactoringType()
        {
            return RefactoringType.RENAME;
        }

        public IRefactoringAttributes getAttributes()
        {
            throw new NotImplementedException();
        }
    }
}
