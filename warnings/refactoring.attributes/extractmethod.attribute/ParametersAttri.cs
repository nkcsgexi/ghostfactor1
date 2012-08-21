using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.refactoring.attributes
{
    public class ParametersAttri : IAttribute
    {
        private String[] paras;
        
        public RefactoringType getRefactoringType()
        {
            return RefactoringType.EXTRACT_METHOD;
        }

        public AttributeKey getKey()
        {
            return AttributeKey.EXTRACT_METHOD_PARAMETERS;
        }

        public object getValue()
        {
            return paras;
        }

        public void setValue(object o)
        {
            paras = (String[])o;
        }
    }

}
