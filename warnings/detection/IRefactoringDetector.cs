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
        IEnumerable<IManualRefactoring> getRefactorings();
    }

    public interface IBeforeAndAfterSourceKeeper
    {
        void setSourceBefore(String source);
        string getSourceBefore();
        void setSourceAfter(String source);
        string getSourceAfter();
    }

    public interface IBeforeAndAfterSyntaxTreeKeeper
    {
        void setSyntaxTreeBefore(SyntaxTree before);
        void setSyntaxTreeAfter(SyntaxTree after);
    }

    public interface IExternalRefactoringDetector : IRefactoringDetector, IBeforeAndAfterSourceKeeper
    {

    }

    public static class RefactoringDetectorFactory
    {
        public static IExternalRefactoringDetector CreateRenameDetector()
        {
            return new RenameDetector();
        }

        public static IExternalRefactoringDetector CreateExtractMethodDetector()
        {
            return new ExtractMethodDetector();
        }

        public static IExternalRefactoringDetector CreateChangeMethodSignatureDetector()
        {
            return  new ChangeMethodSignatureDetector();
        }
    }
}
