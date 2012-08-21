using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.refactoring.attributes
{
    class ModifiersAttri : IAttribute
    {
        public enum MODIFIER
        {
            STATIC,
            READONLY
        }

        private MODIFIER m;

        public RefactoringType getRefactoringType()
        {
            return RefactoringType.EXTRACT_METHOD;
        }

        public AttributeKey getKey()
        {
            return AttributeKey.EXTRACT_METHOD_MODIFIERS;
        }

        public object getValue()
        {
            return m;
        }

        public void setValue(object o)
        {
            m = (MODIFIER) o;
        }
    }
}
