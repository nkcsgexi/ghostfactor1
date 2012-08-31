using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace warnings.analyzer
{
    /* Factory method for returning different types of analyzer, one instance of each anlayzer is enough. */
    public class AnalyzerFactory
    {
        public static IMethodAnalyzer GetMethodAnalyzer()
        {
            return new MethodAnalyzer();
        }

        public static IDocumentAnalyzer GetDocumentAnalyzer()
        {
            return new DocumentAnalyzer();
        }

        public static ISolutionAnalyzer GetSolutionAnalyzer()
        {
            return new SolutionAnalyzer();
        }

        public static IStatementAnalyzer GetStatementAnalyzer()
        {
            return new StatementAnalyzer();
        }

        public static ISyntaxNodeAnalyzer GetSyntaxNodeAnalyzer()
        {
            return new SyntaxNodeAnalyzer();
        }

        public static ISyntaxNodesAnalyzer GetSyntaxNodesAnalyzer()
        {
            return new SyntaxNodesAnalyzer();
        }

        public static IDataFlowAnalyzer GetDataFlowAnalyzer()
        {
            return new DataFlowAnalyzer();
        }

        public static String GetAnalyzersCountInfo()
        {
            StringBuilder sb = new StringBuilder(Environment.NewLine);
            sb.AppendLine("SolutionAnalyzer: " + SolutionAnalyzer.GetCount());
            sb.AppendLine("DocumentAnalyzer: " + DocumentAnalyzer.GetCount());
            sb.AppendLine("MethodAnalyzer: " + MethodAnalyzer.GetCount());
            sb.AppendLine("StatementAnalyzer :" + StatementAnalyzer.GetCount());
            sb.AppendLine("SyntaxNodesAnalyzer: " + SyntaxNodesAnalyzer.GetCount());
            sb.AppendLine("SyntaxNodeAnalyzer: " + SyntaxNodeAnalyzer.GetCount());
            sb.AppendLine("DataFlowAnalyzer: " + DataFlowAnalyzer.GetCount());
            return sb.ToString();
        }

    }
}
