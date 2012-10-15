using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;

namespace warnings.util
{
    public class ASTUtil
    {
        /* Builds Syntax Tree from the source code of a file. */
        public static SyntaxTree GetSyntaxTreeFromSource(String source)
        {
            return SyntaxTree.ParseCompilationUnit(source);
        }

        public static List<MethodDeclarationSyntax> getAllMethodDeclarations(SyntaxTree tree)
        {
            SyntaxNode root = tree.GetRoot();
            var methods = new List<MethodDeclarationSyntax>();
            IEnumerable<SyntaxNode> ite = root.DescendantNodes();
            foreach (SyntaxNode node in ite)
            {
                if (node.Kind == SyntaxKind.MethodDeclaration)
                    methods.Add((MethodDeclarationSyntax)node);
            }
            return methods;
        }

        public static SyntaxNode GetBlockOfMethod(SyntaxNode method)
        {
            return method.DescendantNodes().FirstOrDefault(n => n.Kind == SyntaxKind.Block);
        }

        public static StatementSyntax[] GetStatementsInBlock(SyntaxNode block)
        {
            List<StatementSyntax> stats = new List<StatementSyntax>();
            foreach (SyntaxNode node in block.ChildNodes())
            {
                if (node is StatementSyntax)
                    stats.Add((StatementSyntax)node);
            }
            return stats.ToArray();
        }

        /* Create the semantic model of a given tree. */
        public static SemanticModel CreateSemanticModel(SyntaxTree tree)
        {
            return Compilation.Create("compilation").AddSyntaxTrees(tree)
                .AddReferences(new AssemblyFileReference(typeof(object).Assembly.Location))
                .GetSemanticModel(tree);
        }

        /* Get the classes declared in a syntax tree. */
        public static IEnumerable<ClassDeclarationSyntax> getClassDeclarations(SyntaxTree tree)
        {
            return tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
        }

        /* Get the methods declared in a class declaration. */
        public static IEnumerable<MethodDeclarationSyntax> getMethodDeclarations(ClassDeclarationSyntax classSyntax)
        {
            return classSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>();
        }

        /* Return true if caller is actually calling the callee, otherwise return false. */
        public static bool IsInvoking(SyntaxNode caller, SyntaxNode callee, SyntaxTree tree)
        {
            return GetAllInvocationsInMethod(caller, callee, tree).Any();
        }

        /* Get all the invocations of callee in the body of caller method. */
        public static IEnumerable<InvocationExpressionSyntax> GetAllInvocationsInMethod
            (SyntaxNode caller, SyntaxNode callee, SyntaxTree tree)
        {
            // Where the results are stored.
            var results = new List<InvocationExpressionSyntax>();
            
            // Create semantic model of the given tree.
            SemanticModel model = CreateSemanticModel(tree);

            // Get the entry of callee in the symble table.
            Symbol calleeSymbol = model.GetDeclaredSymbol((MethodDeclarationSyntax)callee);

            // Get all the invocations in the caller.
            var allInvocations = caller.DescendantNodes().
                Where(n => n.Kind == SyntaxKind.InvocationExpression).
                    Select(n => (InvocationExpressionSyntax) n);

            // Among all the invocations, select the ones that are calling the callee symbol.
            return allInvocations.Where(i => model.GetSymbolInfo(i).Symbol == calleeSymbol);
        }

        /* Flatten the caller by replacing a invocation of the callee with the code in the callee. */
        public static String flattenMethodInvocation(MethodDeclarationSyntax caller, 
            MethodDeclarationSyntax callee, InvocationExpressionSyntax invocation)
        {
            // Get the statements in the callee method body except the return statement;
            var statements = GetStatementsInBlock(ASTUtil.GetBlockOfMethod(callee))
                .Where(s => !(s is ReturnStatementSyntax));

            // Combine the statements into one string;
            String replacer = StringUtil.ConcatenateAll("", statements.Select(s => s.GetFullText()).ToArray());
            
            // Get the span of expression, can not invoke invocation.fullspan because {} may exist.
            var span = invocation.Expression.FullSpan;
            String callerString = caller.GetFullText();
            
            // Replace the invocation with the replacer.
            return callerString.Replace(invocation.GetText(), replacer);
        }
    }
}
