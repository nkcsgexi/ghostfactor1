using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.refactoring.attributes
{
    class RenameAttributes : RefactoringAttributes
    {
        public override RefactoringType getRefactoringType()
        {
            return RefactoringType.RENAME;
        }

      
    }
}
