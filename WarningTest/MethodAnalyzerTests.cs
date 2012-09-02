using System;
using System.ComponentModel.Composition;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using warnings.analyzer;
using warnings.util;

namespace WarningTest
{
    [TestClass]
    public class MethodAnalyzerTests
    {
        private IDocument document;

        private IMethodAnalyzer methodAnalyzer = AnalyzerFactory.GetMethodAnalyzer();

        private IDocumentAnalyzer documentAnalyzer = AnalyzerFactory.GetDocumentAnalyzer();
        
        private IEnumerable<SyntaxNode> methods;

        private Logger logger;

        private readonly int METHOD_COUNT = 5;

        public MethodAnalyzerTests()
        {
            var code = TestUtil.getFakeSourceFolder() + "MethodAnalyzerExample.cs";
            var converter = new String2IDocumentConverter();
            document = (IDocument) converter.Convert(FileUtil.readAllText(code), null, null, null);
            logger = NLoggerUtil.getNLogger(typeof (MethodAnalyzerTests));

            documentAnalyzer.SetDocument(document);
            var namespaceDec = documentAnalyzer.GetNamespaceDecalarations().First();
            var classDec = documentAnalyzer.GetClassDeclarations((NamespaceDeclarationSyntax)namespaceDec).First();
            methods = documentAnalyzer.GetMethodDeclarations((ClassDeclarationSyntax)classDec);
        }

        private MethodDeclarationSyntax getMethod(int index)
        {
            Assert.IsTrue(index < METHOD_COUNT);
            return (MethodDeclarationSyntax) methods.ElementAt(index);
        }


        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsNotNull(document);
            Assert.IsNotNull(documentAnalyzer);
            Assert.IsNotNull(methodAnalyzer);
            Assert.IsNotNull(methods);
            Assert.IsTrue(methods.Count() == METHOD_COUNT);
        }

        [TestMethod]
        public void TestMethod2()
        {
            methodAnalyzer.SetMethodDeclaration(getMethod(0));
            var returnType = methodAnalyzer.GetReturnType();
            var para = methodAnalyzer.GetParameters();
            Assert.IsTrue(returnType.Kind == SyntaxKind.PredefinedType);
            Assert.IsTrue(returnType.GetText().Equals("void"));
            Assert.IsFalse(para.Any());
        }

        [TestMethod]
        public void TestMethod3()
        {
            methodAnalyzer.SetMethodDeclaration(getMethod(1));
            var returnType = methodAnalyzer.GetReturnType();
            var para = methodAnalyzer.GetParameters();
            Assert.IsTrue(returnType.Kind == SyntaxKind.PredefinedType);
            Assert.IsTrue(returnType.GetText().Equals("int"));
            Assert.IsFalse(para.Any());
        }

        [TestMethod]
        public void TestMethod4()
        {
            methodAnalyzer.SetMethodDeclaration(getMethod(2));
            var returnType = methodAnalyzer.GetReturnType();
            var para = methodAnalyzer.GetParameters();
            Assert.IsTrue(returnType.Kind == SyntaxKind.IdentifierName);
            Assert.IsTrue(returnType.GetText().Equals("IEnumerable"));
            Assert.IsFalse(para.Any());
        }
        
        [TestMethod]
        public void TestMethod5()
        {
            methodAnalyzer.SetMethodDeclaration(getMethod(3));
            var returnType = methodAnalyzer.GetReturnType();
            var para = methodAnalyzer.GetParameters();
            Assert.IsTrue(returnType.Kind == SyntaxKind.PredefinedType);
            Assert.IsTrue(returnType.GetText().Equals("void"));
            Assert.IsTrue(para.Any());
            Assert.IsTrue(para.First().GetText().Equals("int a"));
        }

        [TestMethod]
        public void TestMethod6()
        {
            methodAnalyzer.SetMethodDeclaration(getMethod(4));
            var returnType = methodAnalyzer.GetReturnType();
            var para = methodAnalyzer.GetParameters();
            Assert.IsTrue(returnType.Kind == SyntaxKind.GenericName);
            Assert.IsTrue(returnType.GetText().Equals("IEnumerable<Object>"));
            logger.Info(methodAnalyzer.DumpTree());
            Assert.IsTrue(para.Any());
            Assert.IsTrue(para.Count() == 5);
            Assert.IsTrue(para.ElementAt(0).GetText().Equals("int a"));
            Assert.IsTrue(para.ElementAt(1).GetText().Equals("int b"));
            Assert.IsTrue(para.ElementAt(2).GetText().Equals("bool c"));
            Assert.IsTrue(para.ElementAt(3).GetText().Equals("Object d"));
            Assert.IsTrue(para.ElementAt(4).GetText().Equals("IEnumerable<int> e"));
        }

        [TestMethod]
        public void TestMethod7()
        {
            methodAnalyzer.SetMethodDeclaration(getMethod(4));
            Assert.IsTrue(methodAnalyzer.HasReturnStatement());
            logger.Info(methodAnalyzer.GetReturnStatement().GetText());
            Assert.IsTrue(methodAnalyzer.GetReturnStatement().GetText().Equals("return Enumerable.Empty<Object>();"));
        }
    }
}
