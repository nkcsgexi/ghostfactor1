using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using warnings.refactoring.attributes;

namespace warnings.refactoring
{
    public interface IRefactoring
    {
        RefactoringType getRefactoringType();
        IRefactoringAttributes getAttributes();
    }
}
