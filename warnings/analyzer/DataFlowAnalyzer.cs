﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;

namespace warnings.analyzer
{
    public interface IDataFlowAnalyzer
    {
        void SetDocument(IDocument document);
        void SetStatements(IEnumerable<SyntaxNode> statements);
        IEnumerable<ISymbol> GetFlowInData();
        IEnumerable<ISymbol> GetFlowOutData();
    }


    internal class DataFlowAnalyzer : IDataFlowAnalyzer
    {
        private IDocument document;

        private ISemanticModel model;

        private IEnumerable<SyntaxNode> statements;


        public void SetDocument(IDocument document)
        {
            this.document = document;
            model = document.GetSemanticModel();
        }


        public void SetStatements(IEnumerable<SyntaxNode> statements)
        {
            this.statements = statements;
        }

        public IEnumerable<ISymbol> GetFlowInData()
        {
            return GetNeededSymbols(statements);
        }

        public IEnumerable<ISymbol> GetFlowOutData()
        {
            IRegionDataFlowAnalysis analysis;

            // Symbols assigned in these statements.
            IEnumerable<ISymbol> assigned = Enumerable.Empty<ISymbol>();
            
            // Iterate each statement.
            foreach (var statement in statements)
            {
                // Get the analysis result.
                analysis = model.AnalyzeStatementDataFlow(statement);

                // Union the symbols that get assigned in this statement.
                assigned.Union(analysis.DataFlowsOut);
            }

            // Get the end position of all these statements 
            var nodesAnalyzer = AnalyzerFactory.GetSyntaxNodesAnalyzer();
            nodesAnalyzer.SetSyntaxNodes(statements);
            int end = nodesAnalyzer.GetEndPosition();

            // Get the method in where the statement lies.
            var statementAnalyzer = AnalyzerFactory.GetStatementAnalyzer();
            statementAnalyzer.SetSyntaxNode(statements.First());
            var method = statementAnalyzer.GetMethodDeclaration();
            
            // Get those statements that are in the same method body but after these statements under analysis.
            var methodAnalyzer = AnalyzerFactory.GetMethodAnalyzer();
            methodAnalyzer.SetMethodDeclaration((MethodDeclarationSyntax)method);
            IEnumerable<SyntaxNode> laterStatements = methodAnalyzer.GetStatementsAfter(end);

            // Get symbols needed in those later statements.
            IEnumerable<ISymbol> needed = GetNeededSymbols(laterStatements);

            return assigned.Except(needed);
        }


        private IEnumerable<ISymbol> GetNeededSymbols(IEnumerable<SyntaxNode> nodes)
        {
            IRegionDataFlowAnalysis analysis;

            // The symbols need in all of the statements
            IEnumerable<ISymbol> flowIns = Enumerable.Empty<ISymbol>();

            // The symbols defined in all the statements.
            IEnumerable<ISymbol> defined = Enumerable.Empty<ISymbol>();

            // Iterate all the statement
            foreach (var node in nodes)
            {
                // Get the analyze result of each statement.
                analysis = model.AnalyzeStatementDataFlow(node);

                // Add current state needed data.
                flowIns = flowIns.Union(analysis.DataFlowsIn);

                // Add current state defined data.
                defined = defined.Union(analysis.VariablesDeclared);
            }

            // Remove all the defined data in all the statements, and return the needed datas.
            return flowIns.Except(defined);

        }



    }
}
