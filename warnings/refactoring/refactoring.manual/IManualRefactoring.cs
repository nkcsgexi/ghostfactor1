using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using warnings.refactoring.attributes;
using warnings.source;

namespace warnings.refactoring
{
    public interface IManualRefactoring 
    {
        void setStartRecord(ICodeHistoryRecord start);
        void setEndRecord(ICodeHistoryRecord end);
    }
}
