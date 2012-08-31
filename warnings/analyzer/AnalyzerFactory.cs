using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.analyzer
{
    /* Factory method for returning different types of analyzer, one instance of each anlayzer is enough. */
    public class AnalyzerFactory
    {
        private static IMethodAnalyzer methoAnalyzer = new MethodAnalyzer();

        private static IDocumentAnalyzer documentAnalyzer = new DocumentAnalyzer();

        private static ISolutionAnalyzer solutionAnalyzer = new SolutionAnalyzer();

        private static IStatementAnalyzer statementAnalyzer = new StatementAnalyzer();

        private static ISyntaxNodeAnalyzer syntaxNodeAnalyzer = new SyntaxNodeAnalyzer();

        private static ISyntaxNodesAnalyzer syntaxNodesAnalyzer = new SyntaxNodesAnalyzer();
        
        private static IDataFlowAnalyzer dataFlowAnalyzer = new DataFlowAnalyzer();

        public static IMethodAnalyzer GetMethodAnalyzer()
        {
            return methoAnalyzer;
        }

        public static IDocumentAnalyzer GetDocumentAnalyzer()
        {
            return documentAnalyzer;
        }

        public static ISolutionAnalyzer GetSolutionAnalyzer()
        {
            return solutionAnalyzer;
        }

        public static IStatementAnalyzer GetStatementAnalyzer()
        {
            return statementAnalyzer;
        }

        public static ISyntaxNodeAnalyzer GetSyntaxNodeAnalyzer()
        {
            return syntaxNodeAnalyzer;
        }

        public static ISyntaxNodesAnalyzer GetSyntaxNodesAnalyzer()
        {
            return syntaxNodesAnalyzer;
        }

        public static IDataFlowAnalyzer GetDataFlowAnalyzer()
        {
            return dataFlowAnalyzer;
        }
    }
}
