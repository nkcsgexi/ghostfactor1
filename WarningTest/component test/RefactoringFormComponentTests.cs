using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using warnings.components;
using warnings.components.ui;

namespace WarningTest.component_test
{
    [TestClass]
    public class RefactoringFormComponentTests
    {
        private IRefactoringFormComponent component;
        
        public RefactoringFormComponentTests()
        {
            component = GhostFactorComponents.refactoringFormComponent;
            component.Start();
        }

        [TestMethod]
        public void TestMethod1()
        {
            component.AddWarning(new string[] { "test0", "test1", "test2", "test3" });
            Thread.Sleep(30000);
        }

        [TestMethod]
        public void TestMethod2()
        {
            component.AddWarning(new string[] { "test0", "test1", "test2", "test3" });
            component.RemoveWarning(new string[] { "test0", "test1", "test2", "test3" });
            Thread.Sleep(3000);
        }
    }
}
