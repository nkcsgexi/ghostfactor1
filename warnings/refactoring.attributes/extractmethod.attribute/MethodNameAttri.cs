using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.refactoring.attributes
{
    class MethodNameAttri : IAttribute
    {
        private String name;

        public RefactoringType getRefactoringType()
        {
            return RefactoringType.EXTRACT_METHOD;
        }

        public AttributeKey getKey()
        {
            return AttributeKey.EXTRACT_METHOD_METHOD_NAME;
        }

        public object getValue()
        {
            return name;
        }

        public void setValue(object o)
        {
            name = (String)o;
        }
    }
}
