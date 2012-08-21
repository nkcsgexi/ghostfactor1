using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.refactoring.attributes
{
    public interface IAttribute
    {
        RefactoringType getRefactoringType();
        AttributeKey getKey();
        Object getValue();
        void setValue(Object o);
    }
}
