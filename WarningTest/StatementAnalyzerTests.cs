using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using warnings.util;

namespace WarningTest
{
    [TestClass]
    public class StatementAnalyzerTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            StatementAnalyzer analyzer = new StatementAnalyzer("print();");
            Assert.IsTrue(analyzer.hasMethodInvocation("print"));
        }
    }
}
