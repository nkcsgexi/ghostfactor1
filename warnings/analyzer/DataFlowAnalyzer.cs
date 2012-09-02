using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Documents;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using warnings.util;

namespace warnings.analyzer
{
    /* Analyzer for one or more statement. */
    public interface IStatementsDataFlowAnalyzer 
    {
        void SetDocument(IDocument document);
        void SetStatements(IEnumerable<SyntaxNode> statements);
        IEnumerable<ISymbol> GetFlowInData();
        IEnumerable<ISymbol> GetFlowOutData();
    }

    /* Analyzer for a single expression. */
    public interface IExpressionDataFlowAnalyzer
    {
        void SetDocument(IDocument document);
        void SetExpression(SyntaxNode expression);
        IEnumerable<ISymbol> GetFlowOut();
        IEnumerable<ISymbol> GetFlowIn();
    }


    internal class StatementsesDataFlowAnalyzer : IStatementsDataFlowAnalyzer
    {
        private static int ANALYZER_COUNT = 0;

        public static int GetCount()
        {
            return ANALYZER_COUNT;
        }

        private ISemanticModel model;

        private IEnumerable<SyntaxNode> statements;

        internal StatementsesDataFlowAnalyzer()
        {  
            Interlocked.Increment(ref ANALYZER_COUNT);
        }

        ~StatementsesDataFlowAnalyzer()
        {
            Interlocked.Decrement(ref ANALYZER_COUNT);
        }

    
        public void SetDocument(IDocument document)
        {
            model = document.GetSemanticModel();
        }


        public void SetStatements(IEnumerable<SyntaxNode> statements)
        {
            this.statements = statements.OrderBy(s => s.Span.Start);
        }

        public IEnumerable<ISymbol> GetFlowInData()
        {

            IRegionDataFlowAnalysis analysis = model.AnalyzeStatementsDataFlow(statements.First(), statements.Last());
            return analysis.DataFlowsIn;
        }

        public IEnumerable<ISymbol> GetFlowOutData()
        {
            IRegionDataFlowAnalysis analysis = model.AnalyzeStatementsDataFlow(statements.First(), statements.Last());
            return analysis.DataFlowsOut;
        }
    }

    internal class ExpressionDataFlowAnalyzer : IExpressionDataFlowAnalyzer
    {
        private static int ANALYZER_COUNT = 0;

        public static int GetCount()
        {
            return ANALYZER_COUNT;
        }

        private SyntaxNode expression;

        private ISemanticModel model;

        internal ExpressionDataFlowAnalyzer()
        {  
            Interlocked.Increment(ref ANALYZER_COUNT);
        }

        ~ExpressionDataFlowAnalyzer()
        {
            Interlocked.Decrement(ref ANALYZER_COUNT);
        }

        public void SetDocument(IDocument document)
        {
            model = document.GetSemanticModel();
        }

        public void SetExpression(SyntaxNode expression)
        {
            this.expression = expression;
        }

        public IEnumerable<ISymbol> GetFlowOut()
        {
            var analysis = model.AnalyzeExpressionDataFlow(expression);
            return analysis.DataFlowsOut;
        }

        public IEnumerable<ISymbol> GetFlowIn()
        {
            var analysis = model.AnalyzeExpressionDataFlow(expression);
            return analysis.DataFlowsIn;
        }
    }
}
