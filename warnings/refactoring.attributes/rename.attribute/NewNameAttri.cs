using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.refactoring.attributes
{
    class NewNameAttri : IAttribute
    {
        private String nName;

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
            return nName;
        }

        public void setValue(Object o)
        {
            nName = (String) o;
        }
    }
}
