using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using warnings.analyzer;
using warnings.util;

namespace warnings.retriever
{
    /* 
     * For retrieving all the renamable things in a given document, the list of renamable things shall keep increasing, right now
     * only solving the most used ones, classdeclarations, methoddeclarations, and variabledeclarators.
     */
    public interface IRenamableRetriever
    {
        /* First all the things in declarations that can be renamed. */
        IEnumerable<SyntaxToken> GetClassDeclarationIdentifiers();
        IEnumerable<SyntaxToken> GetMethodDeclarationIdentifiers();
        IEnumerable<SyntaxToken> GetVariableDeclaratorIdentifiers();
        IEnumerable<SyntaxToken> GetMethodParameterDeclarationIdentifiers();

        /* Next, all the names in refering that can be renamed. */
        IEnumerable<SyntaxNode> GetMemberAccesses();
        IEnumerable<SyntaxToken> GetIdentifierTokens();
    }

    internal class RenamablesRetriever : IRenamableRetriever
    {  
        private readonly Logger logger;

        private readonly SyntaxNode root;

        internal RenamablesRetriever(SyntaxNode root)
        {
            this.logger = NLoggerUtil.getNLogger(typeof (RenamablesRetriever));
            this.root = root;
        }

        public IEnumerable<SyntaxToken> GetMethodDeclarationIdentifiers()
        {
            // All the method declarations.
            // ATTENTION: should not use DescendantNodes(n => n.Kind == ...), it does not yield anything. 
            var declarations = root.DescendantNodes().Where(n => n.Kind == SyntaxKind.MethodDeclaration);
            
            // Select method identifiers from them.
            var list = (from MethodDeclarationSyntax dec in declarations select dec.Identifier).ToList();

            // Logging the retrieved results.
            logger.Info("Method declaration identifiers: " + StringUtil.ConcatenateAll(",", list.Select(n => n.ValueText)));

            // Sorting all the tokens to facilitate comparison.
            return list.OrderBy(n => n.Span.Start).AsEnumerable();
        }

        public IEnumerable<SyntaxToken> GetVariableDeclaratorIdentifiers()
        {
            // All the variable declarators, not declarations. One declaration can include several declarators. 
            // such as int a, b.
            var declarators = root.DescendantNodes().Where(n => n.Kind == SyntaxKind.VariableDeclarator);
            var list = (from VariableDeclaratorSyntax dec in declarators select dec.Identifier).ToList();

            // Logging the retrieved results.
            logger.Info("Variable declaration identifiers: " + StringUtil.ConcatenateAll(",", list.Select(n => n.ValueText)));
            return list.OrderBy(n => n.Span.Start).AsEnumerable();
        }

        public IEnumerable<SyntaxToken> GetMethodParameterDeclarationIdentifiers()
        {
            // Get all the parameters, parameters are in the method's signature (declarations), while arguments are
            // in the method invocation.
            var declartions = root.DescendantNodes().Where(n => n.Kind == SyntaxKind.Parameter);
            var list = (from ParameterSyntax para in declartions select para.Identifier);

            // Logging
            logger.Info("Parameters declaration identifiers: " + StringUtil.ConcatenateAll(",", list.Select(n => n.ValueText)));
            return list.OrderBy(n => n.Span.Start).AsEnumerable();
        }


        public IEnumerable<SyntaxToken> GetClassDeclarationIdentifiers()
        {
            var declarations = root.DescendantNodes().Where(n => n.Kind == SyntaxKind.ClassDeclaration);
            var list = (from ClassDeclarationSyntax dec in declarations select dec.Identifier).ToList();
         
            // Logging the retrieved results.
            logger.Info("Class declaration identifiers: " + StringUtil.ConcatenateAll(",", list.Select(n => n.ValueText)));
            return list.OrderBy(n => n.Span.Start).AsEnumerable();
        }

        /* Get all the expressions for accessing members of classes. A.B.C is an access to members.*/
        public IEnumerable<SyntaxNode> GetMemberAccesses()
        {
            // All the member accessings.
            var accesses = root.DescendantNodes().Where(n => n.Kind == SyntaxKind.MemberAccessExpression);

            // Use nodes analyzer to remove nodes whose parent is also in the list.
            var analyzer = AnalyzerFactory.GetSyntaxNodesAnalyzer();
            analyzer.SetSyntaxNodes(accesses);
            accesses = analyzer.RemoveSubNodes();
            logger.Info("Member Accessings: " + StringUtil.ConcatenateAll(",", accesses.Select(n => n.GetText())));
            return accesses.OrderBy(n => n.Span.Start).AsEnumerable();
        }

        /* Get all identifier tokens in the root. */
        public IEnumerable<SyntaxToken> GetIdentifierTokens()
        {
            // All the identifier tokens
            var names = root.DescendantTokens().Where(n => n.Kind == SyntaxKind.IdentifierToken);
            logger.Info("Identifier tokens: " + StringUtil.ConcatenateAll(",", names.Select(n => n.GetText())));
            return names.OrderBy(n => n.Span.Start).AsEnumerable();
        }
    }
}
