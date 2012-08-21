using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using warnings.refactoring;
using warnings.source;

namespace warnings.refactoring
{
    public interface IAutoRefactoring
    {
        void setCode(String s);
        Boolean checkConditions();
        void performRefactoring();
        String getCodeAfterRefactoring();
        void setStart(int start);
        void setLength(int length);
    }
}
