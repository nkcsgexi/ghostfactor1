﻿using System;
using System.Linq.Expressions;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Roslyn.Compilers.CSharp;
using warnings.util;

namespace WarningTest
{
    [TestClass]
    public class ASTUtilTests
    {
        private static readonly string path =
            @"D:\VS workspaces\BeneWar\warnings\WarningTest\ASTUtilTests.cs";

        private readonly String code;
        private readonly SyntaxTree tree;

        public ASTUtilTests()
        {
            this.code = FileUtil.readAllText(path);
            this.tree = ASTUtil.getSyntaxTreeFromSource(code);
        }



        /* Tests the correct use of isInvoking, it seems not very good at recursive calls.*/
        [TestMethod]
        public void TestMethod1()
        {
            foo();
            ASTUtil.getClassDeclarations(tree);
            ClassDeclarationSyntax classDec = ASTUtil.getClassDeclarations(tree).First();
            IEnumerable<MethodDeclarationSyntax> methodDecs = ASTUtil.getMethodDeclarations(classDec);
            var caller = methodDecs.Where(dec => dec.Identifier.Value.Equals("TestMethod1")).First();
            var callee1 = methodDecs.Where(dec => dec.Identifier.Value.Equals("foo")).First();
            var callee2 = methodDecs.Where(dec => dec.Identifier.Value.Equals("bar")).First();
            Assert.IsTrue(ASTUtil.isInvoking(caller, callee1, tree));
            Assert.IsTrue(ASTUtil.isInvoking(callee1, callee2, tree));
            Assert.IsFalse(ASTUtil.isInvoking(caller, callee2, tree));
            Assert.IsFalse(ASTUtil.isInvoking(callee2, callee1, tree));
        }

        private void foo()
        {
            bar();
        }

        private void bar()
        {
            int i;
            int j;
            i = j = 3;
        }


        [TestMethod]
        public void TestFlattenMethod()
        {
            var caller = tree.Root.DescendentNodes().OfType<MethodDeclarationSyntax>().
                First(m => m.Identifier.Value.Equals("foo"));
            var callee = tree.Root.DescendentNodes().OfType<MethodDeclarationSyntax>().
                First(m => m.Identifier.Value.Equals("bar"));
            var invocation = caller.DescendentNodes().OfType<InvocationExpressionSyntax>().
                First();
            String afterFlatten = ASTUtil.flattenMethodInvocation(caller, callee, invocation);
            Assert.IsTrue(afterFlatten.Contains("private void foo"));
            Assert.IsTrue(afterFlatten.Contains("int i;"));
            Assert.IsTrue(afterFlatten.Contains("int j;"));
            Assert.IsTrue(afterFlatten.Contains("i = j = 3;"));
            Assert.IsFalse(afterFlatten.Contains("private void bar"));
        }


    }
}
