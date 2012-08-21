﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using warnings.source;
using warnings.util;

namespace warnings.refactoring.detection
{
    /*
     * This is a detector for mamual rename refactoring. Setting the source code before and after a time interval, this detector should be able to tell whether
     * a rename refactoring was performed.
     */
    class RenameDetector : IRefactoringDetector, IBeforeAndAfterSourceKeeper
    {
        /* The code before. */
        private String before;
        /* The code after certain time interval. */
        private String after;

        public void setSourceBefore(String source)
        {
            this.before = source;
        }

        public void setSourceAfter(String source)
        {
            this.after = source;
        }

        public bool hasRefactoring()
        {
            return false;
        }

        public IManualRefactoring getRefactoring()
        {
            throw new NotImplementedException();
        }
    }
}
