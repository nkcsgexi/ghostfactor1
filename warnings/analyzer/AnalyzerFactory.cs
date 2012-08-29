using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.analyzer
{
    public class AnalyzerFactory
    {
        private static IMethodAnalyzer methoAnalyzer = new MethodAnalyzer();

        private static IDocumentAnalyzer documentAnalyzer = new DocumentAnalyzer();

        private static ISolutionAnalyzer solutionAnalyzer = new SolutionAnalyzer();

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
    }
}
