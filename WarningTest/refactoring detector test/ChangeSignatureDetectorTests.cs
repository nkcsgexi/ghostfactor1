using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using warnings.refactoring;
using warnings.refactoring.detection;
using warnings.util;

namespace WarningTest
{
    [TestClass]
    public class ChangeSignatureDetectorTests
    {
        private readonly string sourceBefore;
        private readonly string sourceAfter;
        private readonly IExternalRefactoringDetector detector;
        private readonly Logger logger;

        public ChangeSignatureDetectorTests()
        {
            sourceBefore = FileUtil.readAllText(TestUtil.getFakeSourceFolder() + "ChangeMethodSignatureBefore.txt");
            sourceAfter = FileUtil.readAllText(TestUtil.getFakeSourceFolder() + "ChangeMethodSignatureAfter.txt");
            detector = RefactoringDetectorFactory.CreateChangeMethodSignatureDetector();
            detector.setSourceBefore(sourceBefore);
            detector.setSourceAfter(sourceAfter);
            logger = NLoggerUtil.getNLogger(typeof (ChangeSignatureDetectorTests));
        }


        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsNotNull(sourceAfter);
            Assert.IsNotNull(sourceBefore);
            Assert.IsTrue(detector.hasRefactoring());
            var refactorings = detector.getRefactorings();
            Assert.IsTrue(refactorings.Count() == 1);
            var refactoring = (IChangeMethodSignatureRefactoring) refactorings.First();
            var map = refactoring.ParametersMap;
            logger.Info(refactoring.ChangedMethodDeclaration);
            logger.Info(refactoring.ParametersMap.Count());
            Assert.IsTrue(map.Count() == 2);
            
        }
    }
}
