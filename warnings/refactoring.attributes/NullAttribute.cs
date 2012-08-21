using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.refactoring.attributes
{
    class NullAttribute : IAttribute
    {
        public RefactoringType getRefactoringType()
        {
            throw new NotImplementedException();
        }

        public AttributeKey getKey()
        {
            throw new NotImplementedException();
        }

        public object getValue()
        {
            throw new NotImplementedException();
        }

        public void setValue(object o)
        {
            throw new NotImplementedException();
        }
    }
}
