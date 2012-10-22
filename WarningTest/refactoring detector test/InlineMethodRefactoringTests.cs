using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using warnings.refactoring.detection;
using warnings.util;

namespace WarningTest.refactoring_detector_test
{
    [TestClass]
    public class InlineMethodRefactoringTests
    {
        private readonly IExternalRefactoringDetector detector;
        private readonly string codeAfter;
        private readonly string codeBefore;


        public InlineMethodRefactoringTests()
        {
            this.detector = RefactoringDetectorFactory.CreateInlineMethodDetector();
            this.codeBefore = FileUtil.ReadAllText(TestUtil.GetFakeSourceFolder() + "InlineMethodBefore.txt");
            this.codeAfter = FileUtil.ReadAllText(TestUtil.GetFakeSourceFolder() + "InlineMethodAfter.txt");
            detector.SetSourceBefore(codeBefore);
            detector.SetSourceAfter(codeAfter);
        }

        [TestMethod]
        public void TestMethod0()
        {
            Assert.IsNotNull(detector);
            Assert.IsTrue(codeBefore.Length > 0);
            Assert.IsTrue(codeAfter.Length > 0);
        }


        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsTrue(detector.HasRefactoring());
            var refactoring = detector.GetRefactorings().ElementAt(0);
        }

        [TestMethod]
        public void TestMethod2()
        {
            Assert.IsTrue(detector.HasRefactoring());
            var refactoring = detector.GetRefactorings().ElementAt(1);
        }

    }
}
