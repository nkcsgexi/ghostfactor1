﻿using System;
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
        Boolean HasRefactoring();
        IEnumerable<IManualRefactoring> GetRefactorings();
    }

    public interface IBeforeAndAfterSourceKeeper
    {
        void SetSourceBefore(String source);
        string getSourceBefore();
        void SetSourceAfter(String source);
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
