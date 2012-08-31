using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using warnings.util;

namespace warnings.analyzer
{
    /* An anlayzer of the basic structure for the given document. */

    public interface IDocumentAnalyzer
    {
        void SetDocument(IDocument document);
        IEnumerable<SyntaxNode> GetNamespaceDecalarations();
        IEnumerable<SyntaxNode> GetClassDeclarations(NamespaceDeclarationSyntax mamespaceDeclaration);
        IEnumerable<SyntaxNode> GetFieldDeclarations(ClassDeclarationSyntax classDeclaration);
        IEnumerable<SyntaxNode> GetMethodDeclarations(ClassDeclarationSyntax classDeclaration);
        IEnumerable<SyntaxNode> GetVariableDeclarations(MethodDeclarationSyntax methodDeclaration);

        /* Given a node of declaration, returns the symbol in the semantic model. */
        ISymbol GetSymbol(SyntaxNode declaration);

        /* Get the first symbol of different types for testing purpuses. */
        ISymbol GetFirstLocalVariable();
        ISymbol GetFirstClass();
        ISymbol GetFirstNamespace();
        ISymbol GetFirstMethod();
        String DumpSyntaxTree();
    }

    internal class DocumentAnalyzer : IDocumentAnalyzer
    {
        private static int ANALYZER_COUNT = 0;

        public static int GetCount()
        {
            return ANALYZER_COUNT;
        }

        private IDocument document;

        private Logger logger;
                    
        /* the root for the syntax tree of the document. */
        private SyntaxNode root;


        internal DocumentAnalyzer()
        {  
            this.logger = NLoggerUtil.getNLogger(typeof (DocumentAnalyzer));
            Interlocked.Increment(ref ANALYZER_COUNT);
        }

        ~DocumentAnalyzer()
        {
            Interlocked.Decrement(ref ANALYZER_COUNT);
        }


        public void SetDocument(IDocument document)
        {
            this.document = document;
            this.root = (SyntaxNode)document.GetSyntaxRoot();
        }

        public IEnumerable<SyntaxNode> GetNamespaceDecalarations()
        {
            return GetDecendantOfKind(root, SyntaxKind.NamespaceDeclaration);
         }

        public IEnumerable<SyntaxNode> GetClassDeclarations(NamespaceDeclarationSyntax mamespaceDeclaration)
        {
            return GetDecendantOfKind(mamespaceDeclaration, SyntaxKind.ClassDeclaration);
        }

        /* A field declaration can consist of several declarator.*/
        public IEnumerable<SyntaxNode> GetFieldDeclarations(ClassDeclarationSyntax classDeclaration)
        {
            // First get all the field declarations in the class.
            var fields =  GetDecendantOfKind(classDeclaration, SyntaxKind.FieldDeclaration);
            IEnumerable<SyntaxNode> result = null;

            // iterate all these fields
            foreach (FieldDeclarationSyntax field in fields)
            {
                // For each field, find all its containing declarators and cancatenate them.
                if(result == null)
                {
                    result = GetDecendantOfKind(field, SyntaxKind.VariableDeclarator); 
                }
                else
                {
                    result = result.Concat(GetDecendantOfKind(field, SyntaxKind.VariableDeclarator));
                }
            }

            return result;
        }

        public IEnumerable<SyntaxNode> GetMethodDeclarations(ClassDeclarationSyntax classDeclaration)
        {
            return GetDecendantOfKind(classDeclaration, SyntaxKind.MethodDeclaration);
        }

        public IEnumerable<SyntaxNode> GetVariableDeclarations(MethodDeclarationSyntax methodDeclaration)
        {
            return GetDecendantOfKind(methodDeclaration, SyntaxKind.VariableDeclarator);
        }

        public ISymbol GetSymbol(SyntaxNode declaration)
        {
            ISemanticModel model = document.GetSemanticModel();
            
            // Only the following kind of declarations can find their symbol, otherwise throw exception.
            switch (declaration.Kind)
            {
                    case SyntaxKind.NamespaceDeclaration:
                        return model.GetDeclaredSymbol(declaration);

                    case SyntaxKind.ClassDeclaration:
                        return model.GetDeclaredSymbol(declaration);

                    case SyntaxKind.MethodDeclaration:
                        return model.GetDeclaredSymbol(declaration);

                    case SyntaxKind.VariableDeclarator:
                        return model.GetDeclaredSymbol(declaration);
                    
                    default:
                        logger.Fatal("Cannot find symbol to a node that is not a declaration.");
                        break;
            }
            return null;
        }

      

        public string DumpSyntaxTree()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();

            // Iterate each namespace in the file.
            foreach (NamespaceDeclarationSyntax namespaceDec in GetNamespaceDecalarations())
            {
                sb.AppendLine(namespaceDec.Name.PlainName);
                
                // Iterate the classes in each namepace.
                foreach (ClassDeclarationSyntax classDec in GetClassDeclarations(namespaceDec))
                {
                    sb.Append("\t");
                    sb.AppendLine(classDec.Identifier.ValueText);
                    
                    // Iterate the field declarations in each class, print only the first one.
                    foreach (VariableDeclaratorSyntax field in GetFieldDeclarations(classDec))
                    {
                        sb.Append("\t\t");
                        sb.AppendLine(field.Identifier.ValueText);
                    }

                    // Iterate the methods declarations in each class.
                    foreach (MethodDeclarationSyntax method in GetMethodDeclarations(classDec))
                    {
                        sb.Append("\t\t");
                        sb.AppendLine(method.Identifier.ValueText);

                        // Iterate the local variable declarations in each method, print only the first one
                        // in a declaration.
                        foreach (VariableDeclaratorSyntax variable in GetVariableDeclarations(method))
                        {
                            sb.Append("\t\t\t");
                            sb.AppendLine(variable.Identifier.ValueText);
                        }
                    }
                }
            }
            return sb.ToString();
        }

        public ISymbol GetFirstLocalVariable()
        {
            var first_namespace = GetNamespaceDecalarations().First();
            var first_class = GetClassDeclarations((NamespaceDeclarationSyntax)first_namespace).First();
            var first_method = GetMethodDeclarations((ClassDeclarationSyntax)first_class).First();
            var first_variable = GetVariableDeclarations((MethodDeclarationSyntax)first_method).First();
            return GetSymbol(first_variable);
        }
        
        public ISymbol GetFirstMethod()
        {
            var first_namespace = GetNamespaceDecalarations().First();
            var first_class = GetClassDeclarations((NamespaceDeclarationSyntax)first_namespace).First();
            var first_method = GetMethodDeclarations((ClassDeclarationSyntax)first_class).First();
            return GetSymbol(first_method);
        }

        public ISymbol GetFirstClass()
        {
            var first_namespace = GetNamespaceDecalarations().First();
            var first_class = GetClassDeclarations((NamespaceDeclarationSyntax)first_namespace).First();
            return GetSymbol(first_class);
        }

        public ISymbol GetFirstNamespace()
        {
            var first_namespace = GetNamespaceDecalarations().First();
            return GetSymbol(first_namespace);
        }

        private IEnumerable<SyntaxNode> GetDecendantOfKind(SyntaxNode parent, SyntaxKind kind)
        {
            return parent.DescendantNodes().Where(n => n.Kind == kind);
         }

    }


    internal class AnalyzerWalker : SyntaxWalker
    {
        private Logger logger;

        public AnalyzerWalker()
        {
            this.logger = NLoggerUtil.getNLogger(typeof (AnalyzerWalker));
        }

        public override void Visit(SyntaxNode node)
        {
            base.Visit(node);
            logger.Info(node.ToString());
        }
    }
}
