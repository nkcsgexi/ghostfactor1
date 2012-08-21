using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roslyn.Compilers.CSharp;
using warnings.source;
using warnings.refactoring;

namespace warnings.refactoring.detection
{
    public interface IRefactoringDetector
    {
        Boolean hasRefactoring();
        IManualRefactoring getRefactoring();
    }

    public interface IBeforeAndAfterSourceKeeper
    {
        void setSourceBefore(String source);
        void setSourceAfter(String source);
    }

    public interface IBeforeAndAfterSyntaxTreeKeeper
    {
        void setSyntaxTreeBefore(SyntaxTree before);
        void setSyntaxTreeAfter(SyntaxTree after);
    }
}
