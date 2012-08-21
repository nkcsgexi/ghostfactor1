using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.refactoring.attributes
{
    public interface IRefactoringAttributes
    {
        IAttribute getAttribute(AttributeKey key);
        AttributeKey[] getAllKeys();
        IAttribute[] getAllAttributes();
        RefactoringType getRefactoringType();
        void addAttribute(IAttribute att);
        void removeAttribute(IAttribute att);
        Boolean hasAttribute(AttributeKey key);
    }
}
