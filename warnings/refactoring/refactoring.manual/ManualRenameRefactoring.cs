﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using warnings.source;

namespace warnings.refactoring.refactoring.manual
{
    class ManualRenameRefactoring : IManualRefactoring, IRefactoring
    {
        public void setStartRecord(source.ICodeHistoryRecord start)
        {
            throw new NotImplementedException();
        }

        public void setEndRecord(ICodeHistoryRecord end)
        {
            throw new NotImplementedException();
        }

        public void SetEndRecord(source.ICodeHistoryRecord end)
        {
            throw new NotImplementedException();
        }

        public RefactoringType getRefactoringType()
        {
            throw new NotImplementedException();
        }

        public attributes.IRefactoringAttributes getAttributes()
        {
            throw new NotImplementedException();
        }
    }
}
