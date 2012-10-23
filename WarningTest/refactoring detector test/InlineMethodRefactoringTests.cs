﻿using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;
using warnings.conditions;
using warnings.refactoring;
using warnings.refactoring.detection;
using warnings.util;

namespace WarningTest.refactoring_detector_test
{
    [TestClass]
    public class InlineMethodRefactoringTests
    {
        private readonly Logger logger = NLoggerUtil.GetNLogger(typeof (InlineMethodRefactoringTests));
        private readonly IExternalRefactoringDetector detector;
        private readonly IRefactoringConditionsList checkersList;
        private readonly string codeAfter;
        private readonly string codeBefore;
        private readonly IDocument documentAfter;
        private readonly IDocument documentBefore;


        public InlineMethodRefactoringTests()
        {
            var convertor = new String2IDocumentConverter();
            this.detector = RefactoringDetectorFactory.CreateInlineMethodDetector();
            this.checkersList = ConditionCheckingFactory.GetInlineMethodConditionsList();
            this.codeBefore = FileUtil.ReadAllText(TestUtil.GetFakeSourceFolder() + "InlineMethodBefore.txt");
            this.codeAfter = FileUtil.ReadAllText(TestUtil.GetFakeSourceFolder() + "InlineMethodAfter.txt");
            this.documentBefore = (IDocument)convertor.Convert(codeBefore, null, null, null);
            this.documentAfter = (IDocument)convertor.Convert(codeAfter, null, null, null);
            
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
            refactoring.MapToDocuments(documentBefore, documentAfter);
            var computers = checkersList.CheckAllConditions(documentBefore, documentAfter, refactoring);
            Assert.IsTrue(computers.Count() == checkersList.GetCheckerCount());
            Assert.IsTrue(computers.All(c => c is NullCodeIssueComputer));
        }

        [TestMethod]
        public void TestMethod2()
        {
            Assert.IsTrue(detector.HasRefactoring());
            var refactoring = (IInlineMethodRefactoring)detector.GetRefactorings().ElementAt(1);
            refactoring.MapToDocuments(documentBefore, documentAfter);
            var computers = checkersList.CheckAllConditions(documentBefore, documentAfter, refactoring);
            Assert.IsTrue(computers.Count() == checkersList.GetCheckerCount());
            var validComputers = computers.Where(c => c is ValidCodeIssueComputer);
            Assert.IsTrue(validComputers.Any());
            var computer = validComputers.First();
            var issues = ComputerAllCodeIssues(computer, documentAfter).OrderBy(i => i.TextSpan.Start);
            Assert.IsTrue(issues.Any());
            Assert.IsTrue(issues.Count() == refactoring.InlinedStatementsInMethodAfter.Count());
            for (int i = 0; i < issues.Count(); i++)
            {
                var issue = issues.ElementAt(i);
                Assert.IsTrue(IsIssuedTo(issue, refactoring.InlinedStatementsInMethodAfter.ElementAt(i)));
                Assert.IsTrue(issue.Description.StartsWith("Inlined method may change variable"));
            }
            var updatedDocument = UpdateDocumentByCodeAction(documentAfter, issues.First().Actions.First());
            logger.Info(updatedDocument.GetSyntaxRoot().GetText());
        }


        [TestMethod]
        public void TestMethod3()
        {
            Assert.IsTrue(detector.HasRefactoring());
            var refactoring = (IInlineMethodRefactoring) detector.GetRefactorings().ElementAt(2);
            Assert.IsNotNull(refactoring);
            refactoring.MapToDocuments(documentBefore, documentAfter);
            var computers = checkersList.CheckAllConditions(documentBefore, documentAfter, refactoring).
                Where(c => c is ValidCodeIssueComputer);
            Assert.IsTrue(computers.Any());
            Assert.IsTrue(computers.Count() == 1);
            var issues = ComputerAllCodeIssues(computers.First(), documentAfter);
            Assert.IsTrue(issues.Count() == refactoring.InlinedStatementsInMethodAfter.Count());
            for (int i = 0; i < issues.Count(); i++)
            {
                var issue = issues.ElementAt(i);
                Assert.IsTrue(issue.Description.StartsWith("Inlined methodAfter may fail to change variables:"));
            }
            var action = issues.First().Actions.First();
            var updatedDoc = UpdateDocumentByCodeAction(documentAfter, action);
            logger.Info(updatedDoc.GetSyntaxRoot().GetText());
        }



        private IEnumerable<CodeIssue> ComputerAllCodeIssues(ICodeIssueComputer computer, IDocument document)
        {
            var root = (SyntaxNode)document.GetSyntaxRoot();
            return root.DescendantNodes().SelectMany(n => computer.ComputeCodeIssues(document, n));
        }

        private bool IsIssuedTo(CodeIssue issue, SyntaxNode node)
        {
            return issue.TextSpan.Equals(node.Span);
        }


        private IDocument UpdateDocumentByCodeAction(IDocument document, ICodeAction action)
        {
           return action.GetEdit().UpdatedSolution.GetDocument(document.Id);
        }
      
    }
}
