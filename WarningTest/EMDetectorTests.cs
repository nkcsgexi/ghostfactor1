using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using warnings.refactoring.detection;
using warnings.util;

namespace WarningTest
{
    [TestClass]
    public class EMDetectorTests
    {
        private static readonly string fileBefore = 
            TestUtil.getFakeSourceFolder() + "EMDetectorBefore.cs";


        private static readonly string fileAfter = 
            TestUtil.getFakeSourceFolder() + "EMDetectorAfter.cs";

        [TestMethod]
        public void TestMethod1()
        {
            var detector = RefactoringDetectorFactory.createExtractMethodDetector();
            var sourceBefore = FileUtil.readAllText(fileBefore);
            var sourceAfter = FileUtil.readAllText(fileAfter);
            detector.setSourceBefore(sourceBefore);
            detector.setSourceAfter(sourceAfter);
            Assert.IsTrue(detector.hasRefactoring());
        }
    }
}
