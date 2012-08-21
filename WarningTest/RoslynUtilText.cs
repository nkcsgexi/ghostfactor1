using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Services;
using warnings.util;

namespace WarningTest
{
    [TestClass]
    public class RoslynUtilText
    {
        [TestMethod]
        public void TestMethod1()
        {
            ISolution solution = RoslynUtil.GetSolution(TestUtil.getSolutionPath());
            Assert.IsNotNull(solution);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var solution = RoslynUtil.GetSolution(TestUtil.getSolutionPath());
            var project = RoslynUtil.GetProject(solution, "WarningTest");
            Assert.IsNotNull(project);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var solution = RoslynUtil.GetSolution(TestUtil.getSolutionPath());
            var project = RoslynUtil.GetProject(solution, "WarningTest");
            var document = RoslynUtil.GetDocument(project, "ASTUtilTests.cs");
            Assert.IsNotNull(document);
            document = RoslynUtil.GetDocument(project, "EMDetectorAfter.cs");
            Assert.IsNotNull(document);
        }

        [TestMethod]
        public void TestMethod4()
        {
            String updatedString = "Updated String";
            var solution = RoslynUtil.GetSolution(TestUtil.getSolutionPath());
            var project = RoslynUtil.GetProject(solution, "WarningTest");
            var document = RoslynUtil.GetDocument(project, "TryToUpdate.cs");
            Assert.IsNotNull(document);
            solution = RoslynUtil.UpdateDocumentToString(document, updatedString);
            project = RoslynUtil.GetProject(solution, "WarningTest");
            document = RoslynUtil.GetDocument(project, "TryToUpdate.cs");
            Assert.IsNotNull(document);
            Assert.IsTrue(document.GetText().GetText().Equals(updatedString));
        }
    }
}
