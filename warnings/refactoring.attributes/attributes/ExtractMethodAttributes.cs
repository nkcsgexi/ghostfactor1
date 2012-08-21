using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.refactoring.attributes
{
    class ExtraceMethodAttributes : RefactoringAttributes
    {
        public override RefactoringType getRefactoringType()
        {
            return RefactoringType.EXTRACT_METHOD;
        }       
    }
}
