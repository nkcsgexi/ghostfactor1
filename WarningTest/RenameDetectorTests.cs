using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using warnings.refactoring.detection;
using warnings.retriever;
using warnings.util;

namespace WarningTest
{
    [TestClass]
    public class RenameDetectorTests
    {
        private readonly SyntaxNode before;
        
        private readonly SyntaxNode after;

        private readonly IExternalRefactoringDetector detector;
        
        private readonly Logger logger;

        public RenameDetectorTests()
        {
            var sourceBefore = FileUtil.readAllText(TestUtil.getFakeSourceFolder() + "RenameDetectorExampleBefore.txt");
            var sourceAfter = FileUtil.readAllText(TestUtil.getFakeSourceFolder() + "RenameDetectorExampleAfter.txt");
            before = ASTUtil.getSyntaxTreeFromSource(sourceBefore).GetRoot();
            after = ASTUtil.getSyntaxTreeFromSource(sourceAfter).GetRoot();
            detector = RefactoringDetectorFactory.createRenameDetector();
            logger = NLoggerUtil.getNLogger(typeof (RenameDetectorTests));
        }

        private SyntaxNode ModifyIdentifierInAfterSource(SyntaxNode node, int idIndex,String newName)
        {
            var retriever = RetrieverFactory.GetRenamableRetriever();
            retriever.SetRoot(node);
            var tokens = retriever.GetIdentifierTokens();
            Assert.IsTrue(idIndex < tokens.Count());
            TextSpan span = tokens.ElementAt(idIndex).Span;
            string beforeTokenCode = node.GetText().Substring(0, span.Start);
            string afterTokenCode = node.GetText().Substring(span.End);
            return ASTUtil.getSyntaxTreeFromSource(beforeTokenCode + newName + afterTokenCode).GetRoot();
        }



        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsNotNull(before);
            Assert.IsNotNull(after);
            Assert.IsNotNull(detector);
            detector.setSourceBefore(before.GetText());
            detector.setSourceAfter(after.GetText());
            Assert.IsFalse(detector.hasRefactoring());
        }

        [TestMethod]
        public void TestMethod2()
        {
            detector.setSourceBefore(before.GetText());
            for (int i = 0; i < 100; i++)
            {
                // Change one identifier.
                var changedAfter = ModifyIdentifierInAfterSource(after, i, "newNameInjectedForTest");
                detector.setSourceAfter(changedAfter.GetText());
                Assert.IsTrue(detector.hasRefactoring());
            }
        }
        [TestMethod]
        public void TestMethod3()
        {
            detector.setSourceBefore(before.GetText());

            // Change two identifiers.
            var changedAfter = ModifyIdentifierInAfterSource
                (ModifyIdentifierInAfterSource(after, 50, "newNameInjectedForTest"), 60, "newNameInjectedForTest");
            detector.setSourceAfter(changedAfter.GetText());
            Assert.IsFalse (detector.hasRefactoring());

        }

    }
}
