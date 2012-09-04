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

        public static IMethodInvocationAnalyzer GetMethodInvocationAnalyzer()
        {
            return new MethodInvocationAnalyzer();
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

        public static IStatementsDataFlowAnalyzer GetStatementsDataFlowAnalyzer()
        {
            return new StatementsesDataFlowAnalyzer();
        }

        public static IExpressionDataFlowAnalyzer GetExpressionDataFlowAnalyzer()
        {
            return new ExpressionDataFlowAnalyzer();
        }

        public static String GetAnalyzersCountInfo()
        {
            StringBuilder sb = new StringBuilder(Environment.NewLine);
            sb.AppendLine("SolutionAnalyzer: " + SolutionAnalyzer.GetCount());
            sb.AppendLine("DocumentAnalyzer: " + DocumentAnalyzer.GetCount());
            sb.AppendLine("MethodAnalyzer: " + MethodAnalyzer.GetCount());
            sb.AppendLine("MethodInvocationAnalyzer: " + MethodInvocationAnalyzer.GetCount());
            sb.AppendLine("StatementAnalyzer :" + StatementAnalyzer.GetCount());
            sb.AppendLine("SyntaxNodesAnalyzer: " + SyntaxNodesAnalyzer.GetCount());
            sb.AppendLine("SyntaxNodeAnalyzer: " + SyntaxNodeAnalyzer.GetCount());
            sb.AppendLine("StatementsDataFlowAnalyzer: " + StatementsesDataFlowAnalyzer.GetCount());
            sb.AppendLine("ExpressionDataFlowAnalyzer: " + ExpressionDataFlowAnalyzer.GetCount());
            return sb.ToString();
        }

    }
}
