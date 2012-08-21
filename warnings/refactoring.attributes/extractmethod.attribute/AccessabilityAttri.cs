using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.refactoring.attributes
{
    class AccessabilityAttri : IAttribute
    {
        public enum Accessability
        {
            PRIVATE,
            PUBLIC,
            PROTECTED
        }

        private Accessability acc;

        public RefactoringType getRefactoringType()
        {
            return RefactoringType.EXTRACT_METHOD;
        }

        public AttributeKey getKey()
        {
            return AttributeKey.EXTRACT_METHOD_ACCESSABILITY;
        }

        public object getValue()
        {
            return acc;
        }

        public void setValue(object o)
        {
            acc = (Accessability) o;
        }
    }
}
