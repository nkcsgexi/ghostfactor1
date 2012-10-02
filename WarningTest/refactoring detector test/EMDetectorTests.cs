﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
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

        private readonly Logger logger = NLoggerUtil.GetNLogger(typeof (EMDetectorTests));

        [TestMethod]
        public void TestMethod1()
        {
            var detector = RefactoringDetectorFactory.CreateExtractMethodDetector();
            var sourceBefore = FileUtil.ReadAllText(fileBefore);
            var sourceAfter = FileUtil.ReadAllText(fileAfter);
            detector.SetSourceBefore(sourceBefore);
            detector.SetSourceAfter(sourceAfter);
            Assert.IsTrue(detector.HasRefactoring());
            foreach (var refactoring in detector.GetRefactorings())
            {
                logger.Info(refactoring.ToString());
            }
            
        }
    }
}
