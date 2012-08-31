using System;
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
    public class DataFlowAnalyzerTests
    {
        private readonly IDocument document;

        private readonly Logger logger;

        private readonly IEnumerable<SyntaxNode> methods;

        private readonly int METHOD_COUNT = 4;

        private readonly IDataFlowAnalyzer dataFlowanalyzer = AnalyzerFactory.GetDataFlowAnalyzer();

        private readonly IMethodAnalyzer methodAnalyzer = AnalyzerFactory.GetMethodAnalyzer();

        private readonly IList<SyntaxNode> statementsToAnalyze = new List<SyntaxNode>(); 


        public DataFlowAnalyzerTests()
        {
            var code = TestUtil.getFakeSourceFolder() + "DataFlowExample.cs";
            var converter = new String2IDocumentConverter();
            this.document = (IDocument)converter.Convert(FileUtil.readAllText(code), null, null, null);
            logger = NLoggerUtil.getNLogger(typeof(DataFlowAnalyzerTests));

            var analyzer = AnalyzerFactory.GetDocumentAnalyzer();
            analyzer.SetDocument(document);
            var namespaceDec = analyzer.GetNamespaceDecalarations().First();
            var classDec = analyzer.GetClassDeclarations((NamespaceDeclarationSyntax) namespaceDec).First();
            this.methods = analyzer.GetMethodDeclarations((ClassDeclarationSyntax) classDec);

            dataFlowanalyzer.SetDocument(document);
        }
          
        private MethodDeclarationSyntax GetMethodByIndex(int i)
        {
            return (MethodDeclarationSyntax) methods.ElementAt(i);
        }

        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsNotNull(methods);
            Assert.IsTrue(methods.Count() == METHOD_COUNT);
            Assert.IsNotNull(dataFlowanalyzer);
            Assert.IsNotNull(methodAnalyzer);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var method = GetMethodByIndex(0);
            methodAnalyzer.SetMethodDeclaration(method);
            var statements = methodAnalyzer.GetStatements();
            statementsToAnalyze.Clear();
            
            // the while loop.
            statementsToAnalyze.Add(statements.ElementAt(4));
            dataFlowanalyzer.SetStatements(statementsToAnalyze);
            var flowins = dataFlowanalyzer.GetFlowInData().OrderBy(s => s.Name);
            Assert.IsTrue(flowins.Count() == 5);
            Assert.IsTrue(flowins.ElementAt(0).Name.Equals("counter"));
            Assert.IsTrue(flowins.ElementAt(1).Name.Equals("end"));
            Assert.IsTrue(flowins.ElementAt(2).Name.Equals("file"));
            Assert.IsTrue(flowins.ElementAt(3).Name.Equals("lines"));
            Assert.IsTrue(flowins.ElementAt(4).Name.Equals("start"));
            var flowouts = dataFlowanalyzer.GetFlowOutData();
            Assert.IsFalse(flowouts.Any());
           
        }

        [TestMethod]
        public void TestMethod3()
        {
            var method = GetMethodByIndex(1);
            methodAnalyzer.SetMethodDeclaration(method);
            var statements = methodAnalyzer.GetStatements();
            statementsToAnalyze.Clear();
            statementsToAnalyze.Add(statements.ElementAt(2));
            dataFlowanalyzer.SetStatements(statementsToAnalyze);
            var flowins = dataFlowanalyzer.GetFlowInData();
            Assert.IsTrue(flowins.Count() == 1);
            Assert.IsTrue(flowins.First().Name.Equals("b"));
            var flowouts = dataFlowanalyzer.GetFlowOutData();
            Assert.IsTrue(flowouts.Count() == 1);
            Assert.IsTrue(flowouts.First().Name.Equals("b"));
        }
    }
}
