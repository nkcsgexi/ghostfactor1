using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.refactoring.attributes
{
    class OriginalNameAttri : IAttribute
    {
        private String oName;

        public RefactoringType getRefactoringType()
        {
            return RefactoringType.RENAME;
        }

        public AttributeKey getKey()
        {
            return AttributeKey.RENAME_NEW_NAME;
        }

        public object getValue()
        {
            return oName;
        }

        public void setValue(Object o)
        {
            oName = (String) o;
        }
    }
}
