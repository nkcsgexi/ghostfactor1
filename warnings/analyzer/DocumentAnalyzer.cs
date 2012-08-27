using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Roslyn.Services;
using warnings.util;

namespace warnings.analyzer
{
    /* An anlayzer of the basic structure for the given document. */
    internal interface IDocumentAnalyzer
    {
        IEnumerable GetNamespaceDecalarations();
        IEnumerable GetClassDeclarations(NamespaceDeclarationSyntax mamespaceDeclaration);
        IEnumerable GetFieldDeclarations(ClassDeclarationSyntax classDeclaration);
        IEnumerable GetMethodDeclarations(ClassDeclarationSyntax classDeclaration);
        IEnumerable GetVariableDeclarations(MethodDeclarationSyntax methodDeclaration);
        String DumpSyntaxTree();
    }

    public class DocumentAnalyzer : IDocumentAnalyzer
    {
        private IDocument document;

        private Logger logger;
                    
        private SyntaxNode root;

        public DocumentAnalyzer(IDocument document)
        {
            this.document = document;
            this.root = (SyntaxNode)document.GetSyntaxRoot();
            this.logger = NLoggerUtil.getNLogger(typeof (DocumentAnalyzer));
        }

        public IEnumerable GetNamespaceDecalarations()
        {
            return GetDecendantOfKind(root, SyntaxKind.NamespaceDeclaration);
         }

        public IEnumerable GetClassDeclarations(NamespaceDeclarationSyntax mamespaceDeclaration)
        {
            return GetDecendantOfKind(mamespaceDeclaration, SyntaxKind.ClassDeclaration);
        }

        public IEnumerable GetFieldDeclarations(ClassDeclarationSyntax classDeclaration)
        {
            return GetDecendantOfKind(classDeclaration, SyntaxKind.FieldDeclaration);
        }

        public IEnumerable GetMethodDeclarations(ClassDeclarationSyntax classDeclaration)
        {
            return GetDecendantOfKind(classDeclaration, SyntaxKind.MethodDeclaration);
        }

        public IEnumerable GetVariableDeclarations(MethodDeclarationSyntax methodDeclaration)
        {
            return GetDecendantOfKind(methodDeclaration, SyntaxKind.VariableDeclaration);
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
                    foreach (FieldDeclarationSyntax field in GetFieldDeclarations(classDec))
                    {
                        sb.Append("\t\t");
                        sb.AppendLine(field.Declaration.Variables[0].Identifier.ValueText);
                    }

                    // Iterate the methods declarations in each class.
                    foreach (MethodDeclarationSyntax method in GetMethodDeclarations(classDec))
                    {
                        sb.Append("\t\t");
                        sb.AppendLine(method.Identifier.ValueText);

                        // Iterate the local variable declarations in each method, print only the first one
                        // in a declaration.
                        foreach (VariableDeclarationSyntax variable in GetVariableDeclarations(method))
                        {
                            sb.Append("\t\t\t");
                            sb.AppendLine(variable.Variables[0].Identifier.ValueText);
                        }
                    }
                }
            }
            return sb.ToString();
        }

        private IEnumerable GetDecendantOfKind(SyntaxNode parent, SyntaxKind kind)
        {
            return parent.DescendantNodes().Where( n => n.Kind == kind);
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
